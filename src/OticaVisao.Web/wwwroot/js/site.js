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
        const controller = new AbortController();
        request = controller;
        const timeout = setTimeout(() => controller.abort("timeout"), 6000);
        showStatus("Buscando endereço...");

        try {
            const response = await fetch(`https://viacep.com.br/ws/${cep}/json/`, {
                signal: controller.signal,
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
            if (error.name !== "AbortError" || controller.signal.reason === "timeout")
                showStatus("Não foi possível consultar agora. Preencha o endereço manualmente.", "error");
        } finally {
            clearTimeout(timeout);
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
    const preference = window.matchMedia("(prefers-reduced-motion: reduce)");
    let current = 0;
    let hovered = false;
    let focused = false;
    let timer;
    let elapsed = 0;
    let lastTime = null;
    const tick = time => {
        if (lastTime !== null) elapsed += time - lastTime;
        lastTime = time;
        if (elapsed >= 6000) { show(current + 1); return; }
        timer = requestAnimationFrame(tick);
    };
    const schedule = () => {
        cancelAnimationFrame(timer);
        lastTime = null;
        if (!hovered && !focused && !document.hidden && !preference.matches)
            timer = requestAnimationFrame(tick);
    };
    const show = (index, manual = false) => {
        current = (index + photos.length) % photos.length;
        elapsed = 0;
        photos.forEach((photo, i) => {
            photo.classList.toggle("is-current", i === current);
            photo.setAttribute("aria-hidden", String(i !== current));
        });
        tabs.forEach((tab, i) => tab.setAttribute("aria-pressed", String(i === current)));
        if (manual) carousel.querySelector("[data-highlight-status]").textContent = `Foto ${current + 1} de ${photos.length}: ${photos[current].alt}`;
        schedule();
    };
    tabs.forEach((tab, index) => tab.addEventListener("click", () => show(index, true)));
    carousel.querySelector("[data-highlight-prev]").addEventListener("click", () => show(current - 1, true));
    carousel.querySelector("[data-highlight-next]").addEventListener("click", () => show(current + 1, true));
    carousel.addEventListener("mouseenter", () => { hovered = true; schedule(); });
    carousel.addEventListener("mouseleave", () => { hovered = false; schedule(); });
    carousel.addEventListener("focusin", () => { focused = true; schedule(); });
    carousel.addEventListener("focusout", event => { focused = carousel.contains(event.relatedTarget); schedule(); });
    document.addEventListener("visibilitychange", schedule);
    preference.addEventListener("change", schedule);
    carousel.querySelector("[data-highlight-controls]").hidden = false;
    schedule();
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

// Quick catalog and stock actions stay in place instead of reloading the page.
(() => {
    const postForm = async form => {
        const response = await fetch(form.action || window.location.href, {
            method: "POST",
            body: new FormData(form),
            headers: { Accept: "application/json", "X-Requested-With": "XMLHttpRequest" }
        });
        const data = await response.json().catch(() => ({}));
        if (!response.ok || data.success === false) throw new Error(data.message || "Não foi possível concluir a ação.");
        return data;
    };

    document.querySelectorAll("[data-ajax-favorite]").forEach(form => {
        form.addEventListener("submit", async event => {
            event.preventDefault();
            const button = form.querySelector("button");
            if (!button || button.disabled) return;
            button.disabled = true;
            try {
                const data = await postForm(form);
                const active = Boolean(data.isFavorite);
                const label = active ? "Remover dos favoritos" : "Adicionar aos favoritos";
                button.classList.toggle("is-favorite", active);
                button.textContent = active ? "♥" : "♡";
                button.setAttribute("aria-pressed", String(active));
                button.setAttribute("aria-label", label);
                button.title = label;
            } catch (error) {
                window.alert(error.message);
            } finally {
                button.disabled = false;
            }
        });
    });

    document.querySelectorAll("[data-ajax-stock]").forEach(form => {
        form.addEventListener("submit", async event => {
            event.preventDefault();
            const button = form.querySelector("button");
            const row = form.closest("[data-stock-row]");
            if (!button || !row || button.disabled) return;
            button.disabled = true;
            try {
                const data = await postForm(form);
                const quantity = row.querySelector("[data-stock-quantity]");
                quantity.textContent = data.quantity;
                quantity.classList.toggle("stock-control__low", data.quantity <= 2);
                const decrease = row.querySelector('[data-ajax-stock] button[aria-label^="Retirar"]');
                if (decrease) decrease.disabled = data.availableQuantity === 0;
                Object.entries(data.summaries || {}).forEach(([key, value]) => {
                    const target = document.querySelector(`[data-stock-summary="${key}"]`);
                    if (target) target.textContent = value;
                });
            } catch (error) {
                window.alert(error.message);
            } finally {
                if (!button.matches(':disabled') || !button.getAttribute("aria-label")?.startsWith("Retirar")) button.disabled = false;
            }
        });
    });
})();

// Enlarged stock photo preview.
(() => {
    const dialog = document.querySelector("[data-frame-photo-dialog]");
    if (!dialog) return;
    const image = dialog.querySelector("[data-frame-photo-image]");
    const name = dialog.querySelector("[data-frame-photo-name]");
    const code = dialog.querySelector("[data-frame-photo-code]");
    const close = dialog.querySelector("[data-frame-photo-close]");
    document.querySelectorAll("[data-frame-photo]").forEach(trigger => trigger.addEventListener("click", () => {
        image.src = trigger.dataset.framePhoto;
        image.alt = `Foto ampliada de ${trigger.dataset.framePhotoTitle}`;
        name.textContent = trigger.dataset.framePhotoTitle;
        code.textContent = trigger.dataset.framePhotoCode;
        dialog.showModal();
        close.focus();
    }));
    close.addEventListener("click", () => dialog.close());
    dialog.addEventListener("click", event => {
        const bounds = dialog.getBoundingClientRect();
        const outside = event.clientX < bounds.left || event.clientX > bounds.right || event.clientY < bounds.top || event.clientY > bounds.bottom;
        if (outside) dialog.close();
    });
    dialog.addEventListener("close", () => { image.removeAttribute("src"); });
})();
// Accessible mobile navigation.
(() => {
    const toggle = document.querySelector("[data-mobile-menu-toggle]");
    const drawer = document.getElementById("mobile-navigation");
    const closeButton = drawer?.querySelector(".mobile-navigation__close");
    if (!toggle || !drawer || !closeButton) return;

    const desktop = matchMedia("(min-width: 1200px)");
    let closeTimer;
    const setOpen = open => {
        clearTimeout(closeTimer);
        toggle.setAttribute("aria-expanded", String(open));
        toggle.setAttribute("aria-label", open ? "Fechar menu de navegação" : "Abrir menu de navegação");
        toggle.classList.toggle("is-open", open);
        document.body.classList.toggle("mobile-menu-open", open);
        if (open) {
            drawer.hidden = false;
            requestAnimationFrame(() => {
                drawer.classList.add("is-open");
                closeButton.focus();
            });
        } else {
            drawer.classList.remove("is-open");
            closeTimer = setTimeout(() => { drawer.hidden = true; }, 240);
        }
    };

    toggle.addEventListener("click", () => setOpen(toggle.getAttribute("aria-expanded") !== "true"));
    drawer.querySelectorAll("[data-mobile-menu-close]").forEach(button => button.addEventListener("click", () => {
        setOpen(false);
        toggle.focus();
    }));
    drawer.querySelectorAll("a").forEach(link => link.addEventListener("click", () => setOpen(false)));
    document.addEventListener("keydown", event => {
        if (event.key === "Escape" && toggle.getAttribute("aria-expanded") === "true") {
            setOpen(false);
            toggle.focus();
        }
    });
    desktop.addEventListener("change", event => { if (event.matches) setOpen(false); });
})();

// Page-level panel tabs: animated indicator and keyboard navigation.
document.querySelectorAll("[data-panel-tabs]").forEach(tabBar => {
    const links = [...tabBar.querySelectorAll(":scope > a")];
    const indicator = tabBar.querySelector("[data-tab-indicator]");
    if (!links.length || !indicator) return;

    const activeLink = () => tabBar.querySelector(":scope > a.active") ?? links[0];
    const positionIndicator = () => {
        const active = activeLink();
        indicator.style.left = `${active.offsetLeft}px`;
        indicator.style.width = `${active.offsetWidth}px`;

        const left = active.offsetLeft;
        const right = left + active.offsetWidth;
        if (left < tabBar.scrollLeft) tabBar.scrollLeft = left;
        else if (right > tabBar.scrollLeft + tabBar.clientWidth)
            tabBar.scrollLeft = right - tabBar.clientWidth;
    };

    links.forEach((link, index) => link.addEventListener("keydown", event => {
        if (event.key !== "ArrowLeft" && event.key !== "ArrowRight") return;
        event.preventDefault();
        const direction = event.key === "ArrowRight" ? 1 : -1;
        const target = links[(index + direction + links.length) % links.length];
        target.focus();
        target.click();
    }));

    requestAnimationFrame(positionIndicator);
    window.addEventListener("resize", positionIndicator, { passive: true });
    if ("ResizeObserver" in window) new ResizeObserver(positionIndicator).observe(tabBar);
});

// Administrator team search, invitation form and bulk selection.
(() => {
    const rows = [...document.querySelectorAll("[data-team-row]")];
    if (!rows.length) return;
    const search = document.querySelector("[data-team-search]");
    const selectAll = document.querySelector("[data-team-select-all]");
    const checkboxes = rows.map(row => row.querySelector("[data-team-checkbox]")).filter(Boolean);
    const bulk = document.querySelector("[data-team-bulk]");
    const count = document.querySelector("[data-team-selected-count]");
    const resultCount = document.querySelector("[data-team-result-count]");
    const empty = document.querySelector("[data-team-empty]");
    const create = document.querySelector("[data-admin-create]");
    const createToggle = document.querySelector("[data-admin-create-toggle]");
    const syncSelection = () => {
        const visibleBoxes = checkboxes.filter(box => !box.closest("[data-team-row]").hidden);
        const selected = checkboxes.filter(box => box.checked);
        bulk.hidden = selected.length === 0;
        count.textContent = selected.length;
        selectAll.checked = selected.length > 0 && selected.length === visibleBoxes.length;
        selectAll.indeterminate = selected.length > 0 && !selectAll.checked;
    };
    selectAll?.addEventListener("change", () => {
        checkboxes.forEach(box => { if (!box.closest("[data-team-row]").hidden) box.checked = selectAll.checked; });
        syncSelection();
    });
    checkboxes.forEach(box => box.addEventListener("change", syncSelection));
    search?.addEventListener("input", () => {
        const query = search.value.trim().toLocaleLowerCase("pt-BR");
        let visible = 0;
        rows.forEach(row => {
            row.hidden = query.length > 0 && !row.dataset.teamQuery.includes(query);
            if (!row.hidden) visible++;
        });
        resultCount.textContent = visible + " registro(s)";
        empty.hidden = visible !== 0;
        syncSelection();
    });
    const setCreateOpen = open => {
        create.classList.toggle("is-open", open);
        createToggle.setAttribute("aria-expanded", String(open));
        if (open) create.querySelector("input")?.focus();
    };
    createToggle?.addEventListener("click", () => setCreateOpen(!create.classList.contains("is-open")));
    document.querySelector("[data-admin-create-close]")?.addEventListener("click", () => {
        setCreateOpen(false);
        createToggle.focus();
    });
    syncSelection();
})();
