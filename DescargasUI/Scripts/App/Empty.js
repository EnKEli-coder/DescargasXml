

function openSideMenu() {
    var menuBlock = document.getElementById('menu-blocker');
    var sideMenu = document.querySelector('#side-menu');
    sideMenu.classList.add('open');
    menuBlock.classList.add("active");
}

function closeSideMenu() {
    var menuBlock = document.getElementById('menu-blocker');
    var sideMenu = document.querySelector('#side-menu');
    sideMenu.classList.remove('open');
    menuBlock.classList.remove("active");
}