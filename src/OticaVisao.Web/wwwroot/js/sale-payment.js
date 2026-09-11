(() => {
    const payment = document.querySelector("[data-payment-method]");
    const installments = document.querySelector("[data-installments]");
    if (!payment || !installments) return;

    const update = () => {
        const credit = payment.value === "CreditCard";
        installments.hidden = !credit;
        if (!credit) installments.querySelector("select").value = "1";
    };

    payment.addEventListener("change", update);
    update();
})();
