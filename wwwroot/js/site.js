
document.addEventListener("DOMContentLoaded", function () {
	var path = window.location.pathname;

	var menuLinks = document.querySelectorAll('.main-nav .menu a');

	menuLinks.forEach(function (link) {
		var linkPath = link.getAttribute('href');

		if (path.startsWith(linkPath)) {

			link.parentElement.classList.add('active');
		}
	});
});