const rentalModal = document.getElementById("rental-modal");
const rentalFormContainer = document.getElementById("rental-form-container");

// Открыть модалку аренды с загрузкой формы
window.openRentalModal = async function (carId) {
    if (!rentalModal || !rentalFormContainer) {
        console.error("Rental modal elements not found!");
        return;
    }

    rentalFormContainer.innerHTML = "Загрузка...";
    rentalModal.classList.add("active");
    rentalModal.style.display = "flex"; // "flex" для центрирования

    try {
        const response = await fetch(`/Rental/Rental?carId=${carId}`);
        if (!response.ok) throw new Error("Автомобиль недоступен для аренды");

        const html = await response.text();
        rentalFormContainer.innerHTML = html;
    } catch (error) {
        rentalFormContainer.innerHTML = `<div style="color:red;">${error.message}</div>`;
    }
};


window.closeRentalModal = function () {
    if (rentalModal) {
        rentalModal.classList.remove("active");
        rentalModal.style.display = "none";
        rentalFormContainer.innerHTML = "";
    }
};

// Закрытие по клику вне окна
window.addEventListener("click", function (event) {
    if (event.target === rentalModal) {
        window.closeRentalModal();
    }
});
