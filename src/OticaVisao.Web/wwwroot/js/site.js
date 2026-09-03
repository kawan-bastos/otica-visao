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
