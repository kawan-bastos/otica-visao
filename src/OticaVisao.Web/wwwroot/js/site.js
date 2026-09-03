// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.querySelectorAll("[data-cep-lookup]").forEach((cepInput) => {
    const form = cepInput.closest("form");
    const status = form?.querySelector("[data-cep-status]");
    const fields = {
        street: form?.querySelector('[data-address-field="street"]'),
        neighborhood: form?.querySelector('[data-address-field="neighborhood"]'),
        city: form?.querySelector('[data-address-field="city"]'),
        state: form?.querySelector('[data-address-field="state"]')
    };
    let lastCep = "";
    let request;

    const showStatus = (message, state = "") => {
        if (!status) return;
        status.textContent = message;
        status.dataset.state = state;
    };

    const lookup = async () => {
        const cep = cepInput.value.replace(/\D/g, "");
        if (cep.length !== 8) {
            showStatus(cep.length ? "Digite os 8 números do CEP." : "", cep.length ? "error" : "");
            return;
        }
        if (cep === lastCep) return;

        request?.abort();
        request = new AbortController();
        showStatus("Buscando endereço...");

        try {
            const response = await fetch(`https://viacep.com.br/ws/${cep}/json/`, {
                signal: request.signal,
                headers: { Accept: "application/json" }
            });
            if (!response.ok) throw new Error("Falha na consulta");

            const address = await response.json();
            if (address.erro) {
                showStatus("CEP não encontrado. Preencha o endereço manualmente.", "error");
                return;
            }

            if (fields.street) fields.street.value = address.logradouro ?? "";
            if (fields.neighborhood) fields.neighborhood.value = address.bairro ?? "";
            if (fields.city) fields.city.value = address.localidade ?? "";
            if (fields.state) fields.state.value = address.uf ?? "";
            lastCep = cep;
            showStatus("Endereço preenchido. Confira os dados e informe o número.", "success");
        } catch (error) {
            if (error.name !== "AbortError")
                showStatus("Não foi possível consultar agora. Preencha o endereço manualmente.", "error");
        }
    };

    cepInput.addEventListener("input", () => {
        const digits = cepInput.value.replace(/\D/g, "").slice(0, 8);
        cepInput.value = digits.length > 5 ? `${digits.slice(0, 5)}-${digits.slice(5)}` : digits;
        if (digits.length === 8) lookup();
        else {
            lastCep = "";
            showStatus(digits.length ? "Digite os 8 números do CEP." : "", digits.length ? "error" : "");
        }
    });
    cepInput.addEventListener("blur", lookup);
});

document.querySelectorAll("[data-sale-includes-lenses]").forEach((toggle) => {
    const form = toggle.closest("form");
    const lensFields = form?.querySelectorAll("[data-sale-lens-fields]") ?? [];
    const updateLensFields = () => {
        lensFields.forEach((group) => {
            group.hidden = !toggle.checked;
            group.querySelectorAll("input, select").forEach((field) => field.disabled = !toggle.checked);
        });
    };
    toggle.addEventListener("change", updateLensFields);
    updateLensFields();
});

// Progressive enhancement: content remains visible without JavaScript or animation support.
document.querySelectorAll("[data-highlights]").forEach(carousel => {
    const photos = [...carousel.querySelectorAll("[data-highlight-photo]")];
    const tabs = [...carousel.querySelectorAll("[data-highlight-index]")];
    const pause = carousel.querySelector("[data-highlight-pause]");
    const preference = window.matchMedia("(prefers-reduced-motion: reduce)");
    let current = 0;
    let paused = preference.matches;
    let hovered = false;
    let focused = false;
    let timer;
    const schedule = () => {
        clearTimeout(timer);
        if (!paused && !hovered && !focused && !document.hidden && !preference.matches)
            timer = setTimeout(() => show(current + 1), 6000);
    };
    const show = (index, manual = false) => {
        current = (index + photos.length) % photos.length;
        photos.forEach((photo, i) => {
            photo.classList.toggle("is-current", i === current);
            photo.setAttribute("aria-hidden", String(i !== current));
        });
        tabs.forEach((tab, i) => tab.setAttribute("aria-pressed", String(i === current)));
        if (manual) carousel.querySelector("[data-highlight-status]").textContent = `Foto ${current + 1} de ${photos.length}: ${photos[current].alt}`;
        schedule();
    };
    const updatePause = () => {
        pause.textContent = paused ? "Reproduzir" : "Pausar";
        pause.setAttribute("aria-label", paused ? "Iniciar troca automática" : "Pausar troca automática");
        schedule();
    };
    tabs.forEach((tab, index) => tab.addEventListener("click", () => show(index, true)));
    carousel.querySelector("[data-highlight-prev]").addEventListener("click", () => show(current - 1, true));
    carousel.querySelector("[data-highlight-next]").addEventListener("click", () => show(current + 1, true));
    pause.addEventListener("click", () => { paused = !paused; updatePause(); });
    carousel.addEventListener("mouseenter", () => { hovered = true; schedule(); });
    carousel.addEventListener("mouseleave", () => { hovered = false; schedule(); });
    carousel.addEventListener("focusin", () => { focused = true; schedule(); });
    carousel.addEventListener("focusout", event => { focused = carousel.contains(event.relatedTarget); schedule(); });
    document.addEventListener("visibilitychange", schedule);
    preference.addEventListener("change", () => { paused = true; updatePause(); });
    carousel.querySelector("[data-highlight-controls]").hidden = false;
    pause.hidden = preference.matches;
    preference.addEventListener("change", () => { pause.hidden = preference.matches; });
    updatePause();
});

// One-time entrances on public pages.
(() => {
    const preference = window.matchMedia("(prefers-reduced-motion: reduce)");
    if (preference.matches || !Element.prototype.animate || !("IntersectionObserver" in window)) return;

    const animations = new Set();
    const reveal = (element, delay = 0) => {
        if (preference.matches) return;
        const animation = element.animate([
            { opacity: 0.35, translate: "0 18px" },
            { opacity: 1, translate: "0 0" }
        ], { duration: 550, delay, easing: "cubic-bezier(.2,.65,.3,1)", fill: "backwards" });
        animations.add(animation);
        animation.finished.then(() => animations.delete(animation), () => animations.delete(animation));
    };

    document.querySelectorAll(".hero__content > *").forEach((element, index) => reveal(element, index * 65));
    const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (!entry.isIntersecting) return;
            reveal(entry.target);
            observer.unobserve(entry.target);
        });
    }, { threshold: 0.08 });

    document.querySelectorAll(
        ".catalog-preview .section-heading, .frame-card, .offer__card, .journey__steps > article, .about__grid, .contact__card"
    ).forEach(element => observer.observe(element));

    preference.addEventListener("change", event => {
        if (!event.matches) return;
        observer.disconnect();
        animations.forEach(animation => animation.cancel());
        animations.clear();
    });
})();
