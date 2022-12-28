
// scrolls the page to the top
function js_scrollToTop() {
    
    window.scrollTo({ top: 0, behavior: 'smooth' });

}


// scrolls the page to the id
function js_scrollToId(id) {

    const element = document.getElementById(id);

    if (element instanceof HTMLElement) {

        element.scrollIntoView
            (
                {
                    behavior: "smooth",
                    block: "start",
                    inline: "nearest"
                }

        );
        
        window.location.hash = id;

    }

}

