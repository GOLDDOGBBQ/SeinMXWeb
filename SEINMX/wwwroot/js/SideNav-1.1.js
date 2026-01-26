$(document).ready(function () {
    //Fixing jQuery Click Events for the iPad
    var ua = navigator.userAgent,
        eventType  = (ua.match(/iPad/i)) ? "touchstart" : "click";

    const ELEMENTOS = '.OpenSideNav';
    const BotonesCierre = '.CloseSideNav';
    const SideNavSelector = '.sidenav'; // Asegúrate que todos los side nav tengan esta clase

    if ($(ELEMENTOS).length > 0) {
        $(ELEMENTOS).addClass("Cursor");

        $(ELEMENTOS).on(eventType , function () {

            const sidenavID = $(this).attr('target');     
            if (sidenavID != null) {
                const Element = document.getElementById('Side' + sidenavID);
                if (Element != undefined) {
                    Element.style.display = "block";
                }
               
            }
        });
    }
    if ($(BotonesCierre).length > 0) {
        $(BotonesCierre).on(eventType , function () {
            $(this).attr("href", "javascript:void(0)")        
            const sidenavID = $(this).data('itn-sidenav');
            const frameID = $(this).data('itn-frame');
            if (sidenavID != null) {     
                document.getElementById(sidenavID).style.display = "none";
            }
            if (frameID != null) {      
                document.getElementById(frameID).src = "/img/Carga.gif";
            }
        });
    }

    $(document).on(eventType, function (e) {
        const $target = $(e.target);

        // Ignorar si el clic fue dentro de cualquier SideNav o botón que lo abre
        if (
            $target.closest(SideNavSelector).length === 0 &&
            !$target.is(ELEMENTOS) &&
            $target.closest(ELEMENTOS).length === 0
        ) {
            $(SideNavSelector).each(function () {
                if ($(this).is(':visible')) {
                    $(this).hide();
                }
            });
        }
    });


})