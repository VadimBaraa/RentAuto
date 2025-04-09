const menuItems = document.querySelectorAll('.navigation-menu li');

menuItems.forEach(item => {
    item.addEventListener('mouseenter', () => {
        menuItems.forEach(otherItem => {
            otherItem.classList.remove('left');
        });
        const activeItem = document.querySelector('.navigation-menu li.active');
        if (activeItem && item.offsetLeft < activeItem.offsetLeft) {
            item.classList.add('left');
        }
    });
});

