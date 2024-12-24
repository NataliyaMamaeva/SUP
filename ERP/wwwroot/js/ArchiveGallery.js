//document.addEventListener('DOMContentLoaded', () => {
//    const currentPage = document.body.dataset.page;
//    if (currentPage === 'Gallery') {
//        const galleries = document.querySelectorAll('.gallery');
//        let currentImageIndex = 0;
//        let currentGallery = null;

//        // Создаём полноэкранную галерею
//        const fullscreenOverlay = document.createElement('div');
//        fullscreenOverlay.classList.add('fullscreen-overlay');
//        fullscreenOverlay.innerHTML = `
//        <span class="close-btn">&times;</span>
//        <span class="prev-btn">&lt;</span>
//        <span class="next-btn">&gt;</span>
//        <img src="" alt="">
//    `;
//        document.body.appendChild(fullscreenOverlay);

//        const fullscreenImage = fullscreenOverlay.querySelector('img');
//        const closeButton = fullscreenOverlay.querySelector('.close-btn');
//        const prevButton = fullscreenOverlay.querySelector('.prev-btn');
//        const nextButton = fullscreenOverlay.querySelector('.next-btn');

//        // Функция открытия изображения
//        function openFullscreen(gallery, index) {
//            currentGallery = gallery;
//            currentImageIndex = index;
//            const images = gallery.querySelectorAll('img');

//            console.log(images);

//            fullscreenImage.src = images[index].src;
//            fullscreenOverlay.classList.add('active');
//        }

//        // Функция закрытия галереи
//        function closeFullscreen() {
//            fullscreenOverlay.classList.remove('active');
//        }

//        // Функция переключения изображений
//        function changeImage(direction) {
//            if (!currentGallery) return;

//            const images = currentGallery.querySelectorAll('img');
//            currentImageIndex = (currentImageIndex + direction + images.length) % images.length;
//            fullscreenImage.src = images[currentImageIndex].src;
//        }

//        // Обработчики событий
//        galleries.forEach(gallery => {
//            gallery.addEventListener('click', (e) => {

//                //console.log("gallery click");

//                if (e.target.tagName === 'IMG') {

//                    console.log("gallery click IMG");
//                    //console.log(e.target);

//                    const images = [...gallery.querySelectorAll('img')];
//                    const index = images.indexOf(e.target);
//                    openFullscreen(gallery, index);
//                }
//            });
//        });

//        closeButton.addEventListener('click', closeFullscreen);
//        prevButton.addEventListener('click', () => changeImage(-1));
//        nextButton.addEventListener('click', () => changeImage(1));
//        fullscreenOverlay.addEventListener('click', (e) => {
//            if (e.target === fullscreenOverlay) closeFullscreen();
//        });

//        document.addEventListener('keydown', (e) => {
//            if (!fullscreenOverlay.classList.contains('active')) return;

//            if (e.key === 'Escape') closeFullscreen();
//            if (e.key === 'ArrowLeft') changeImage(-1);
//            if (e.key === 'ArrowRight') changeImage(1);
//        });
//    }
//});
