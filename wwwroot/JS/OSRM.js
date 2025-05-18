
async function getRouteFromOSRM(start, end) {
    const url = `http://localhost:5000/route/v1/driving/${start[1]},${start[0]};${end[1]},${end[0]}?overview=full&geometries=geojson`;

    try {
        const response = await fetch(url);
        const data = await response.json();

        if (data.routes && data.routes.length > 0) {
            return data.routes[0].geometry.coordinates.map(c => [c[1], c[0]]);
        } else {
            alert("Маршрут не найден");
            return null;
        }
    } catch (error) {
        console.error("Ошибка OSRM:", error);
        return null;
    }
}

async function drawRoute(startCoords, endCoords, car) { // добавляем параметр car

    // Проверяем, есть ли у car свойство ImageUrl
    const carIconUrl = car && car.ImageUrl ? car.ImageUrl : '/images/default-car-icon.png'; // запасной вариант

    const url = `/api/osrm/route?startLat=${startCoords[0]}&startLon=${startCoords[1]}&endLat=${endCoords[0]}&endLon=${endCoords[1]}`;

    console.log("Отправка запроса на маршрут:", url);

    const response = await fetch(url);
    if (!response.ok) {
        console.error("Ошибка при получении маршрута:", await response.text());
        return;
    }

    const data = await response.json();
    const coordinates = data.routes[0].geometry.coordinates;

    const routePoints = coordinates.map(([lon, lat]) => [lat, lon]);

    if (routePolyline) {
        myMap.geoObjects.remove(routePolyline);
    }

    routePolyline = new ymaps.Polyline(routePoints, {}, {
        strokeColor: "#0000FF",
        strokeWidth: 4,
        strokeOpacity: 0.8
    });
    console.log("Ответ от сервера:", data);

    myMap.geoObjects.add(routePolyline);

    // Создаем метку-машинку
    if (carPlacemark) {
        myMap.geoObjects.remove(carPlacemark);
    }

    let currentIndex = 0;
    carPlacemark = new ymaps.Placemark(routePoints[0], {}, {
        iconLayout: 'default#image',
        iconImageHref: carIconUrl, // здесь используем ссылку на иконку машины
        iconImageSize: [32, 32],
        iconImageOffset: [-16, -16]
    });
    myMap.geoObjects.add(carPlacemark);

    // Запускаем анимацию
    const animationSpeed = 50; // миллисекунд между точками
    const step = () => {
        if (currentIndex < routePoints.length) {
            carPlacemark.geometry.setCoordinates(routePoints[currentIndex]);
            currentIndex++;
            setTimeout(step, animationSpeed);
        }
    };
    step();
}



function init() {
    if (myMap) {
        myMap.events.add('click', function (e) {
            const coords = e.get('coords');

            if (!startPoint) {
                startPoint = coords;
                startPlacemark = new ymaps.Placemark(coords, { iconCaption: "Старт" }, { preset: 'islands#greenDotIcon' });
                myMap.geoObjects.add(startPlacemark);
            } else if (!endPoint) {
                endPoint = coords;
                finishPlacemark = new ymaps.Placemark(coords, { iconCaption: "Финиш" }, { preset: 'islands#redDotIcon' });
                myMap.geoObjects.add(finishPlacemark);

                drawRoute(startPoint, endPoint); // Прокладываем маршрут
            }
            drawCarIfSelected();
        });
    } else {
        console.error("Карта myMap еще не инициализирована.");
    }
}

