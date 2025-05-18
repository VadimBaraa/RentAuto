document.addEventListener('DOMContentLoaded', () => {
    const locationButton = document.getElementById('getLocationButton');
    const clearButton = document.getElementById('clearRouteButton');
    const locationResult = document.getElementById('locationResult');
    const mapContainer = document.getElementById('map');


    // Инициализация карты
    if (window.ymaps && mapContainer) {
        
        ymaps.ready(() => {
            // Значения по умолчанию — центр Воронежа
            let mapCenter = [51.6720400, 39.1843000];
            let zoomLevel = 12;

            // Если координаты переданы через ViewBag
            if (typeof centerLat !== 'undefined' && centerLat !== null && centerLon !== null) {
                mapCenter = [parseFloat(centerLat), parseFloat(centerLon)];
                zoomLevel = 16; // Увеличенный зум для показа конкретного авто
            }

            myMap = new ymaps.Map('map', {
                center: mapCenter,
                zoom: zoomLevel,
                controls: ['zoomControl']
            });

            if (clearButton) {
                clearButton.addEventListener('click', () => {
                    if (myMap) {
                        removeStartAndFinishMarkers(); // Удаляем все объекты с карты
                    }

                    // Сброс переменных
                    startPoint = null;
                    endPoint = null;
                    routeLine = null;
                    routePolyline = null;

                    console.log("Маршрут и точки удалены");
                });
            }

            init(); // Твоя логика для добавления меток и маршрутов
        });
            
    } 

    

    if (locationButton) {
        locationButton.addEventListener('click', () => {
            locationButton.classList.add('loading');
            
            if (!navigator.geolocation) {
                showError('Геолокация не поддерживается вашим браузером');
                return;
            }

            // Показываем индикатор загрузки
            locationResult.textContent = 'Определение местоположения...';
            locationResult.classList.remove('error');
            locationResult.classList.add('visible');

            navigator.geolocation.getCurrentPosition(
                async (position) => {
                    try {
                        const latitude = position.coords.latitude;
                        const longitude = position.coords.longitude;

                        const response = await fetch(
                            `/Home/GetLocation?lat=${latitude}&lon=${longitude}`,
                            {
                                method: 'GET',
                                headers: {
                                    'Accept': 'application/json'
                                }
                            }
                        );

                        if (!response.ok) {
                            throw new Error(`Ошибка сервера: ${response.status}`);
                        }

                        const data = await response.json();
                        
                        locationResult.classList.add('visible');
                        locationResult.innerHTML = `
                            <div>
                                <strong>Ваше местоположение:</strong><br>
                                ${data.address ? `<small>${data.address}</small>` : ''}
                            </div>
                        `;

                        if (myMap) {
                            if (userPlacemark) {
                                myMap.geoObjects.remove(userPlacemark);
                            }
                            userPlacemark = new ymaps.Placemark(
                                [latitude, longitude],
                                {
                                    hintContent: 'Вы здесь',
                                    balloonContent: `Широта: ${latitude}°<br>Долгота: ${longitude}°`
                                },
                                {
                                    preset: 'islands#blueCircleDotIcon'
                                }
                            );
                            myMap.geoObjects.add(userPlacemark);
                            myMap.setCenter([latitude, longitude], 15, {
                                duration: 300
                            });
                        }

                    } catch (error) {
                        showError(error.message);
                    } finally {
                        locationButton.classList.remove('loading');
                    }
                },
                (error) => {
                    let errorMessage;
                    switch (error.code) {
                        case error.PERMISSION_DENIED:
                            errorMessage = "Вы отказали в доступе к геолокации";
                            break;
                        case error.POSITION_UNAVAILABLE:
                            errorMessage = "Информация о местоположении недоступна";
                            break;
                        case error.TIMEOUT:
                            errorMessage = "Время запроса геолокации истекло";
                            break;
                        default:
                            errorMessage = "Произошла неизвестная ошибка";
                    }
                    showError(errorMessage);
                    locationButton.classList.remove('loading');
                }
            );
        });
    }

    function showError(message) {
        if (locationResult) {
            locationResult.classList.add('visible', 'error');
            locationResult.innerHTML = `
                <div>❌ Ошибка: ${message}</div>
            `;
        }
        console.error(message);
    }
});

function ClearMap() {
    if (clearButton) {
        clearButton.addEventListener('click', () => {
            if (myMap) {
                removeStartAndFinishMarkers(); // Удаляем все объекты с карты
            }
    
            // Сброс переменных
            startPoint = null;
            endPoint = null;
            routeLine = null;
            routePolyline = null;
                
            console.log("Маршрут и точки удалены");
        });
    }
}

function removeStartAndFinishMarkers() {
    if (startPlacemark) {
        myMap.geoObjects.remove(startPlacemark);  // Удаляем метку старта
        startPlacemark = null;  // Сбрасываем переменную
    }
    
    if (finishPlacemark) {
        myMap.geoObjects.remove(finishPlacemark);  // Удаляем метку финиша
        finishPlacemark = null;  // Сбрасываем переменную
    }

    if (routePolyline) {
        myMap.geoObjects.remove(routePolyline);  // Удаляем метку старта
        routePolyline = null;  // Сбрасываем переменную
    }
}

document.addEventListener('DOMContentLoaded', () => {
    if (!sessionStorage.getItem('locationSent')) {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(function(position) {
                const lat = position.coords.latitude;
                const lon = position.coords.longitude;

                const newUrl = `${window.location.pathname}?lat=${lat}&lon=${lon}`;

                sessionStorage.setItem('locationSent', 'true'); // чтобы не зациклиться

                window.location.href = newUrl;
            });
        } else {
            console.warn("Геолокация не поддерживается браузером.");
        }
    }
});


