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
