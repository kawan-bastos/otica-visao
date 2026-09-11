// Aceita vírgula ou ponto nos valores monetários e nos graus.
if (window.jQuery?.validator) {
    $.validator.methods.number = function (value, element) {
        return this.optional(element) || /^-?\d+(?:[.,]\d+)?$/.test(value);
    };

    const parseBrazilianNumber = value => {
        if (typeof value === "number") return value;
        const text = String(value);
        return Number(text.includes(",") ? text.replace(/\./g, "").replace(",", ".") : text);
    };

    $.validator.methods.range = function (value, element, parameters) {
        if (this.optional(element)) return true;
        const number = parseBrazilianNumber(value);
        return Number.isFinite(number)
            && number >= parseBrazilianNumber(parameters[0])
            && number <= parseBrazilianNumber(parameters[1]);
    };

    $.extend($.validator.messages, {
        number: "Informe um número válido.",
        range: $.validator.format("Informe um valor entre {0} e {1}.")
    });
}
