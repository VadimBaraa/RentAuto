document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".start-payment").forEach(button => {
        button.addEventListener("click", async () => {
            const carId = button.getAttribute("data-car-id");

            const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            const token = tokenElement ? tokenElement.value : "";

            const response = await fetch("/Payment/CreateCheckoutSession", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                body: JSON.stringify({ carId: parseInt(carId) })
            });

            if (!response.ok) {
                alert("Ошибка при создании платежа");
                return;
            }

            const data = await response.json();
            const stripe = Stripe(data.publishableKey);
            await stripe.redirectToCheckout({ sessionId: data.id });
        });
    });
});
