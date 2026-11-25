"use strict";

document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll(".post-gallery-container").forEach((gallery) => {

    const dialog = gallery.querySelector("dialog");
    const cards = [...gallery.querySelectorAll(".post-image-card")];

    if (!dialog || !cards.length) return;

    const images = cards.map(card => {
      const img = card.querySelector("img");

      return { src: img.src, alt: img.alt };
    });

    const modalImage = dialog.querySelector(".modal-image");
    const caption = dialog.querySelector(".image-caption");

    const previousButton = dialog.querySelector(".modal-prev");
    const nextButton = dialog.querySelector(".modal-next");

    let currentIndex = 0;

    // Open and populate modal
    function openModal(index) {
      currentIndex = index;
      updateModal();

      if (!dialog.open) {
        dialog.showModal();
      }
    }

    // Update modal image
    function updateModal() {
      const image = images[currentIndex];

      modalImage.src = image.src;
      modalImage.alt = image.alt;

      caption.textContent = `(${currentIndex + 1} / ${images.length})`;
    }

    // Navigate images
    function navigate(direction) {
      if (!images.length) return;

      currentIndex = (currentIndex + direction + images.length) % images.length;

      updateModal();
    }

    // Invoker command fallback
    cards.forEach((card, index) => {
      card.addEventListener("click", () => {
        openModal(index);
      });
    });

    // Modal navigation
    previousButton.addEventListener("click", () => navigate(-1));
    nextButton.addEventListener("click", () => navigate(1));

    // Keyboard navigation
    dialog.addEventListener("keydown", (event) => {

      if (event.key === "ArrowRight") {
        event.preventDefault();
        navigate(1);
      }

      if (event.key === "ArrowLeft") {
        event.preventDefault();
        navigate(-1);
      }
    });

    // Close on backdrop click
    dialog.addEventListener("click", (event) => {
      if (event.target === dialog) {
        dialog.close();
      }
    });

    // Clean up
    dialog.addEventListener("close", () => {
      modalImage.removeAttribute("src");
      caption.textContent = "";
    });
  });
});
