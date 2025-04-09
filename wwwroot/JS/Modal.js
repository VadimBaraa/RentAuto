const hamburgerMenu = document.getElementById('hamburger-menu');
const navigationMenu = document.getElementById('navigation-menu');
const openModalButton = document.getElementById('open-modal'); 
const closeModalButton = document.getElementById('close-modal');
const loginModal = document.getElementById('login-modal');

if (openModalButton) {
    openModalButton.addEventListener('click', () => {
        loginModal.style.display = 'block';
    });
}

if (closeModalButton) {
    closeModalButton.addEventListener('click', () => {
        loginModal.style.display = 'none';
    });
}


window.addEventListener('click', (event) => {
    if (event.target === loginModal) {
        loginModal.style.display = 'none';
    }
});

if (hamburgerMenu){
    hamburgerMenu.addEventListener('click', function() {
        navigationMenu.classList.toggle('active');
    });
}
