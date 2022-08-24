function checkear(element) {
    event.stopPropagation();
    var _parent = element;
    var nextli = element.parentElement.nextElementSibling.childNodes;

    if (element.checked) {
        nextli.forEach(child => {
            child.childNodes.forEach(nodo => {
                if (nodo.nodeName == "INPUT") {
                    nodo.checked = true;
                }
            });
        });
    }
    else {
        nextli.forEach(child => {
            child.childNodes.forEach(nodo => {
                if (nodo.nodeName == "INPUT") {
                    nodo.checked = false;
                }
            });
        });
    }
}

function seleccionar(element) {
    var parentinput = element.parentElement.parentElement.previousElementSibling.childNodes;
    var siblingsli = element.parentElement.parentElement.childNodes;

    if (element.checked) {
        parentinput.forEach(child => {
            if (child.nodeName == "INPUT") {
                child.checked = true;
            }
        });
    }
    else {
        var status = true;
        siblingsli.forEach(child => {
            child.childNodes.forEach(option => {
                if (option.nodeName == "INPUT") {
                    if (option.checked) {
                        status = false;
                    }
                }
            });
        });

        if (status) {
            parentinput.forEach(child => {
                if (child.nodeName == "INPUT") {
                    child.checked = false;
                }
            });
        }
    }
}

function activar(element) {
    
    element.classList.toggle("active");

    if (element.classList.contains("active")) {
        element.classList.replace("fa-plus", "fa-minus");
    } else {
        element.classList.replace("fa-minus", "fa-plus");
    }

    element.parentElement.nextElementSibling.childNodes.forEach(child => {
        if (child.nodeName == "LI") {
            child.classList.toggle("active");
        } 
    });
}