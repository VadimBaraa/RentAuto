const addCarModal = document.getElementById("add-car-modal");
const addCarForm = document.getElementById("add-car-form");

// Делаем функции глобальными — доступны из onclick=""
window.openAddCarModal = function () {
    if (addCarModal) {
        addCarModal.classList.add("active");
        addCarModal.style.display = "block";
    } else {
        console.error("Modal not found!");
    }
};

window.closeAddCarModal = function () {
    if (addCarModal) {
        addCarModal.classList.remove("active");
        addCarModal.style.display = "none";
    } else {
        console.error("Modal not found!");
    }
};

// Закрытие по клику вне окна
window.addEventListener("click", function (event) {
    if (event.target === addCarModal) {
        window.closeAddCarModal();
    }
});

// Обработка формы добавления автомобиля
addCarForm?.addEventListener("submit", async (e) => {
    e.preventDefault();

    const formData = new FormData(addCarForm);
    const json = Object.fromEntries(formData.entries());
    json.IsAvailable = formData.get("IsAvailable") === "on"; // checkbox


    const response = await fetch("/Car/Add", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(json),
    });

    if (response.ok) {
        const successMessage = document.getElementById("add-car-success");
        if (successMessage) successMessage.style.display = "block";
        addCarForm.reset();
        setTimeout(() => {
            window.closeAddCarModal();
            window.location.reload();
        }, 1500);
    } else {
        alert("Ошибка при добавлении авто");
    }
});


document.querySelectorAll('.card-button').forEach(button => {
    button.addEventListener('click', () => {
        const lat = button.getAttribute('data-lat');
        const lon = button.getAttribute('data-lon');
        const carId = button.getAttribute('data-car-id'); // если нужно

        localStorage.setItem('selectedCarLat', lat);
        localStorage.setItem('selectedCarLon', lon);
        window.location.href = '/Home/Index'; // или куда у тебя карта
    });
});


