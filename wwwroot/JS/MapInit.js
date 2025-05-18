document.addEventListener('DOMContentLoaded', function () {
    ymaps.ready(function () {
        fetch('/Car/GetCarCoordinates')
            .then(response => response.json())
            .then(cars => {
                cars.forEach(car => {
                    const templateHtml = document.getElementById('circle-icon-template').innerHTML;
                    const circleIconLayout = ymaps.templateLayoutFactory.createClass(templateHtml);

                    const placemark = new ymaps.Placemark([car.lat, car.lon], {
                        balloonContent: `
                            <div>
                                <strong>${car.title}</strong><br/>
                                <a href="https://yandex.ru/maps/?pt=${car.lon},${car.lat}&z=16&l=map" target="_blank">
                                    Открыть в Яндекс.Картах
                                </a>
                            </div>
                        `,
                        image: car.imageUrl
                    }, {
                        iconLayout: circleIconLayout,
                        iconShape: {
                            type: 'Circle',
                            coordinates: [20, 20],
                            radius: 20
                        },
                        iconOffset: [-20, -20]
                    });

                    myMap.geoObjects.add(placemark);
                });

                // Центрирование на выбранную машину
                const lat = parseFloat(sessionStorage.getItem('Latitude'));
                const lon = parseFloat(sessionStorage.getItem('Longitude'));

                console.log('Получены из sessionStorage:', lat, lon);

                if (!isNaN(lat) && !isNaN(lon)) {
                    myMap.setCenter([lat, lon], 16);

                    const selectedPlacemark = new ymaps.Placemark([lat, lon], {
                        balloonContent: 'Выбранный автомобиль'
                    }, {
                        preset: 'islands#redAutoIcon'
                    });

                    myMap.geoObjects.add(selectedPlacemark);
                    console.log('Метка выбранного авто добавлена');
                } else {
                    console.log('Нет координат в sessionStorage для центрирования карты');
                }

            });
            
    });
});


        // Переносим сюда обработчики кнопок
        // Код загрузки карты и меток — оставить как есть...

        // Вне ymaps.ready и вне других DOMContentLoaded
        document.addEventListener('DOMContentLoaded', function () {
            const buttons = document.querySelectorAll('.show-on-map-btn');
            buttons.forEach(button => {
                button.addEventListener('click', function (event) {
                    event.preventDefault();

                    const lat = this.getAttribute('data-lat');
                    const lon = this.getAttribute('data-lon');
                    const url = this.getAttribute('data-url');

                    console.log('Нажата кнопка "Показать на карте"');
                    console.log('lat:', lat, 'lon:', lon, 'url:', url);

                    if (lat && lon && url) {
                        sessionStorage.setItem('Latitude', lat);
                        sessionStorage.setItem('Longitude', lon);
                        console.log('Координаты сохранены в sessionStorage');

                        const fullUrl = `${url}?lat=${lat}&lon=${lon}`;
                        console.log('Редирект на:', fullUrl);

                        window.location.href = fullUrl;
                    } else {
                        console.error('Отсутствуют координаты или URL.');
                    }
                });
            });
        });
