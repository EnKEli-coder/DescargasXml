/**
 *Variables globales:
 *      - archivos: List<archivo>, contiene los archivos cuyos datos seran registrados en el servidor.
 *      - unidades: Objeto Json, lista de ramos y partidas que forman el universo.
 *      - moduloActivo: int, define el modulo que se renderiza, si es 0 el fileUploader, si es 1 el historial de cargas.
 *      - block: HTMLElement, div que bloquea la pagina al cargar.
 *      - modal: HTMLElement, div del modal.
 *      - modalBlock: HTMLElement, div que bloquea la pagina cuando se abre el modal.
 *      - blurScreen: HTMLElement, div al que se le aplica el efecto de blur.
 *      - form: HTMLElement, formulario que contiene el fileUploader.
 *      - inputBusqueda: HTMLElement, input de busqueda de las partidas.
 *      - seleccionUnidad: HTMLElement, Select de las unidades.
 *      - btnToggleSelect: HTMLElement, Boton del select que lo abre y cierra.
 *      - registroAEliminar: HTMLElement, elemento de la lista del historial a eliminar.
 *      - delayTimer: int, tiempo de retraso para escribir en el buscador.
 */ 
var archivos = [];
var unidades = {};
var moduloActivo = 0;
var block = document.getElementById("block");
var modal = document.getElementById("modal");
var modalBlock = document.getElementById("modal-block");
var blurScreen = document.getElementById("file-drop-container");
var form;
var inputBusqueda;
var seleccionUnidad;
var btnToggleSelect;
var registroAEliminar;
var delayTimer;
var listaScrollObserver;
var listaScroll = {
    enEspera : false,
    pagina : 0
}
var paginadoLista = 0;




/**
 * Se ejecuta al inicio y renderiza el modulo por defecto.
 */
$(document).ready(
    renderModulo);

/** Obtiene la lista de unidades y ramos.*/
function obtenerUnidades() {

    axios({
        method: 'post',
        url: '/SubirXmls/obtenerUnidades/',
        headers: {
            "Content-Type": "application/json"
        }
    }).then(function (response) {
        unidades = JSON.parse(response.data);
        block.classList.remove("active");
    }).catch(function (error) {
        openModal("Error", "Ha ocurrido un error, intente de nuevo mas tarde.", "closeModal()");
        block.classList.remove("active");
        limpiar();
    });
}

/**
 * Configura las variables para el funcionamiento del FileUploader.
 */
function configFileInput() {
    form = document.querySelector("form");
    inputFile = form.querySelector(".file-input");

    form.addEventListener("click", () => {
        inputFile.click();
    })
    ;['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        form.addEventListener(eventName, preventDefaults, false);
    })
    ;['dragenter', 'dragover'].forEach(eventName => {
        form.addEventListener(eventName, highlight, false);
    })
    ;['dragleave', 'drop'].forEach(eventName => {
        form.addEventListener(eventName, unhighlight, false);
    })

    inputBusqueda = document.getElementById("busqueda");
    inputBusqueda.addEventListener('input', busqueda);

    seleccionUnidad = document.querySelector(".select-form");
    btnToggleSelect = document.querySelector(".select-btn");

    btnToggleSelect.addEventListener("click", toggleSeleccion, false);

    form.addEventListener('drop', handleDrop, false);

    inputFile.onchange = ({ target }) => {
        handleFiles(target.files);
        target.value = null;
    }

}

function preventDefaults(e) {
    e.preventDefault();
    e.stopPropagation();
}

/**
 * Cambia el style del area de Drag&Drop al entrar al area con un archivo.
 * @param {any} e
 */
function highlight(e) {
    form.classList.add('highlight');
}

/**
 * Cambia el style del area de Drag&Drop al dejar el area con un archivo.
 * @param {Event} e - evento que ejecuta la funcion.
 */
function unhighlight(e) {
    form.classList.remove('highlight');
}

/**Abre y cierra el Select de unidades al darle click. */
function toggleSeleccion() {
    seleccionUnidad.classList.toggle("active");
}

/**
 * Cambia el modulo al darle click al boton de cambio
 */
function cambiarModulo() {
    moduloActivo = moduloActivo == 0 ? 1 : 0;
    renderModulo();
}

/**
 * Renderiza las vistas parciales de los modulos.
 */
function renderModulo() {
    var historyIcon = document.getElementById("history-icon");
    var uploadIcon = document.getElementById("upload-icon");
    var moduloContainer = document.getElementById("modulos");
    block.classList.add("active");
    axios.post('/SubirXmls/renderModulo/',{
        modulo: moduloActivo
    }).then(function (response) {
        if (moduloActivo == 0) {
            historyIcon.style.display = 'flex';
            uploadIcon.style.display = 'none';
            moduloContainer.innerHTML = response.data;
            configFileInput();
            obtenerUnidades();
        } else {
            historyIcon.style.display = 'none';
            uploadIcon.style.display = 'flex';
            moduloContainer.innerHTML = response.data;
            block.classList.remove("active");
        }
    }).catch(function (error) {
        openModal("Error", "Ha ocurrido un error, intente de nuevo mas tarde.", "closeModal()");
        block.classList.remove("active");
        limpiar();
    });
}

/**
 * Abre un modal de confirmacion para eliminar un elemento del historial de cargas.
 * @param {HTMLElement} element - li seleccionado con la informacion del registro.
 */
function eliminarModal(element) {
    registroAEliminar = element;
    openModal("Eliminar", "¿Esta seguro de eliminar este registro?", "eliminarRegistro()");
}

/** Elimina el registro seleccionado del historial de cargas.*/
function eliminarRegistro() {
    closeModal();
    block.classList.add("active");
    var fecha = registroAEliminar.parentElement.previousElementSibling.querySelector("#fecha-registro").innerHTML;
    //fecha.replace(" ", "T");
    axios.post("/SubirXmls/EliminarRegistro", {
        fecha: fecha
    }).then(function (response) {

        if (response.data = true) {
            registroAEliminar.parentElement.parentElement.remove();
            block.classList.remove("active");
        } else {
            block.classList.remove("active");
            errorModal.classList.add("active");
        }
        registroAEliminar = null;

    }).catch(function (error) {
        openModal("Error", "Ha ocurrido un error, intente de nuevo mas tarde.", "closeModal()");
        block.classList.remove("active");
        registroAEliminar = null;
    });
}


/**
 * Filtra la lista de unidades en base a la busqueda realizada. 
 */
function busqueda() {

    clearTimeout(delayTimer);
    delayTimer = setTimeout(function () {
        var buscar = document.getElementById("busqueda");
        var busqueda = buscar.value.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
        var lista = document.getElementById("opciones");

        var listaFiltrada = {};

        if (busqueda == "") {
            $('.opciones li').remove();
        } else {

           listaFiltrada = unidades.filter(partida => partida.toLowerCase().includes(busqueda.toLowerCase()));

            if (listaFiltrada.length > 0) {

                $('.opciones li').remove();

                for (var unidad in listaFiltrada) {
                    var li = document.createElement("li");
                    li.innerHTML = listaFiltrada[unidad];
                    li.addEventListener("click", selectUnidad, false);

                    lista.insertAdjacentElement('beforeend', li);
                }
            } else {
                $('.opciones li').remove();
                var li = document.createElement("li");
                li.innerHTML = "No hay resultados";

                lista.insertAdjacentElement('beforeend', li);
            }
        }
        
    }, 100)
}

/**
 * Da funcionalidad a la seleccion de una opcion de la lista de unidades.
 * @param {Event} event - evento que se ejecuta al seleccionar una unidad
 */
function selectUnidad(event) {
    var seleccion = document.getElementById("span-select");
    seleccion.innerHTML = event.currentTarget.innerHTML;
    seleccion.title = event.currentTarget.innerHTML;
    toggleSeleccion();
    activarBoton();
}

/**
 * Obtiene los archivos que son dropeados en el area de Drag&Drop.
 * @param {Event} e - Evento que contiene los archivos dropeados.
 */
function handleDrop(e) {
    handleFiles(e.dataTransfer.files);
}

/**
 * Eellena la lista de archivos.
 * @param {File} files - archivos que se suben a traves del Request
 */
function handleFiles(files) {

    if (files.length > 0) {
        var indiceNuevo = 0;
        archivos.forEach(eleLista => {
            eleLista.id = indiceNuevo;
            indiceNuevo++;
        });

        for (var i = 0; i < files.length; i++) {
            if (files[i].type == "text/xml") {
                var archivo = {}
                archivo.id = indiceNuevo;
                archivo.file = files[i];
                archivos.push(archivo);
                indiceNuevo++;
            }
        }
        generarLista(archivos);
    }
}

function paginarLista() {

    var i = paginadoLista;
    var queue = [];

    for (i; i < paginadoLista + 50 && i < archivos.length; i++) {
        queue.push({
            id: archivos[i].id,
            nombre: archivos[i].file.name.slice(0,15) + '...' + archivos[i].file.name.slice(-15)
        })
    }
    paginadoLista = i;

    return queue;
}

/**
 * Renderiza la lista de archivos en la cola.
 */
function generarLista() {
    var cantidad = document.getElementById("cantidad-archivos");
    var block = document.getElementById("block");
    var listaContenedor = document.querySelector("#queue-container");
    var listaArchivos = document.querySelector("#lista-archivos");

    var options = {
        root: listaContenedor,
        rootMargin: '0px',
        threshold: 0.5
    }

    block.classList.add("active");

    var queue = paginarLista();
    
    //const formData = new FormData();
    //archivos.forEach(archivo => {
    //    formData.append(archivo.id, archivo.file);
    //});

    axios({
        method: 'post',
        url: '/SubirXmls/_ListaArchivos',
        data: JSON.stringify({queue: queue}),
        headers: {
            "Content-Type": "application/json"
        }
    })
    .then(function (response) {
        listaArchivos.innerHTML = response.data;
        cantidad.innerHTML = archivos.length +" archivos";
        activarBoton();

        listaScrollObserver = new IntersectionObserver(cargarLista, options);

        var cargarMas = document.querySelector("#cargar-mas");
        listaScrollObserver.observe(cargarMas)

        block.classList.remove("active");

    }).catch(function (error) {
        openModal("Error", "Ha ocurrido un error, intente de nuevo mas tarde.", "closeModal()");
        block.classList.remove("active");
        limpiar();
    });
}

function cargarLista(entries, observer) {

    var listaArchivos = document.querySelector("#lista-archivos");

    entries.forEach(async (entry) => {
        if (!listaScroll.enEspera && entry.intersectionRatio > 0 && paginadoLista < archivos.length) {
            listaScroll.enEspera = true;
            var loaderLista = document.querySelector("#loading-archivos");
            loaderLista.style.display = 'flex';

            var listaContenedor = document.querySelector("#queue-container");

            listaContenedor.scroll({
                top: listaContenedor.scrollHeight,
                left: 0,
                behavior: 'smooth'
            })

            var queue = paginarLista();

            try {
                var response = await axios({
                    method: 'post',
                    url: '/SubirXmls/_ListaArchivos',
                    data: JSON.stringify({ queue: queue }),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            } catch (error) {
                openModal("Error", "Ha ocurrido un error, intente de nuevo mas tarde.", "closeModal()");
                loaderLista.style.display = 'none'
                listaScroll.enEspera = false
                limpiar();
            }
            
            //.then(function (response) {
            //    listaArchivos.innerHTML += response.data;
            //}).catch(function (error) {
            //    openModal("Error", "Ha ocurrido un error, intente de nuevo mas tarde.", "closeModal()");
            //    loaderLista.style.display = 'none'
            //    listaScroll.enEspera = false
            //    limpiar();
            //});

            setTimeout(() => {
                listaArchivos.innerHTML += response.data;
                listaScroll.enEspera = false
                loaderLista.style.display = 'none';
            }, 1000)
        }
    })
    
}

/**
 * Activa el boton de Subir si hay una unidad y al menos un archivo en cola.
 */
function activarBoton() {
    var button = document.getElementById("btn-list");
    var seleccion = document.getElementById("span-select");
    var opciones = document.getElementsByClassName("objeto-lista")

    if (opciones.length > 0 && seleccion.innerHTML != "Selección unidad") {
        button.removeAttribute("disabled", "");
    }
}

/*
 * Envia los archivos al controlador para su carga.
 */
function enviarArchivos() {

    var unidad = document.getElementById("span-select");
    var block = document.getElementById("block");
    var resumeModal = document.getElementById("resume-modal");
    var mensaje = document.getElementById("mensaje-resume");
    var registrados = document.getElementById("registered");
    var noRegistrados = document.getElementById("no-registered");

    block.classList.add("active");

    const formData = new FormData();
    archivos.forEach(archivo => {
        formData.append(archivo.id, archivo.file);
    });
    formData.append("unidad", unidad.innerHTML);
    axios({
        method: 'post',
        url: '/SubirXmls/GuardarRegistro/',
        data: formData,
        responseType: 'json',
        headers: {
            "Content-Type": "multipart/form-data",
        }
    }).then(function (response) {
        var json = response.data;
        var countNuevos = 0;

        if (json.length > 0) {
            for (var i in json) {
                if (json[i].guardar) {
                    var span = document.createElement("span");
                    span.innerText = json[i].nombre;
                    registrados.appendChild(span);
                    countNuevos++;
                } else {
                    var span = document.createElement("span");
                    span.innerText = json[i].nombre;
                    noRegistrados.appendChild(span);
                }
            }

            if (countNuevos > 0) {
                mensaje.innerHTML = "Completado exitosamente! " + countNuevos + " nuevos registros.";
            } else {
                mensaje.innerHTML = "No hay nuevos registros";
            }

            
        } else {
            mensaje.innerHTML = "Ha ocurrido un error, intente de nuevo más tarde, si persiste contacte al desarrollador."
        }


        if (registrados.classList.contains("active")) {
            registrados.classList.remove("active");
        }

        if (noRegistrados.classList.contains("active")) {
            noRegistrados.classList.remove("active");
        }
        resumeModal.classList.add("active");
        modalBlock.classList.add("active");
        blurScreen.classList.toggle("blur");
        block.classList.remove("active");

        limpiar();

    }).catch(function (error) {
        openModal("Error", "Ha ocurrido un error, intente de nuevo mas tarde.", "closeModal()");
        block.classList.remove("active");
        limpiar();
    });
}

/**
 * Elimina un elemento de la lista de archivos.
 * @param {HTMLElement} elemento - opcion de la lista a eliminar.
 */
function eliminar(elemento) {

    var button = document.getElementById("btn-list");
    var opcion = elemento.parentElement;
    var cantidad = document.getElementById("cantidad-archivos");
    var index = archivos.map(x => {
        return x.id;
    });

    var aEliminar = index.indexOf(parseInt(opcion.id));

    archivos.splice(aEliminar, 1);

    opcion.remove();
    cantidad.innerHTML = archivos.length + " archivos";

    paginadoLista -= 1;

    if (archivos.length == 0) {
        button.setAttribute("disabled", "");
    }

}

/**
 * Desactiva el boton de Subir, reinicia el select y vacia la lista de archivos.
 */
function limpiar() {
    var button = document.getElementById("btn-list");
    var selectSpan = document.getElementById("span-select");
    var buscar = document.getElementById("busqueda");
    var cantidad = document.getElementById("cantidad-archivos");
    var listaArchivos = document.querySelector("#lista-archivos")

    paginadoLista = 0;

    button.setAttribute("disabled", "");
    selectSpan.innerHTML = "Selección unidad";

    buscar.value = "";
    cantidad.innerHTML = "0 archivos";
    busqueda();

    archivos = [];
    listaArchivos.innerHTML = "";
}

/**
 * Abre un modal con el titulo, contenido y un boton de accion dados como parametros.
 * @param {string} titulo - El titulo del modal.
 * @param {string} contenido - Texto descriptivo del modal.
 * @param {string} accion - Funcion que se ejecuta al darle click al boton.
 */
function openModal(titulo, contenido, accion) {
    var title = document.getElementById("titulo-modal");
    var content = document.getElementById("texto-modal")
    var btn = document.getElementById("btn-modal");

    title.innerText = titulo;
    content.innerText = contenido;
    btn.setAttribute("onclick", accion);
    modal.classList.add("active");
    modalBlock.classList.add("active");
    blurScreen.classList.toggle("blur");
}

/**
 * Cierra el modal de resultados.
 */
function closeResume() {
    var resume = document.getElementById("resume-modal");
    resume.classList.remove("active");
    blurScreen.classList.toggle("blur");
    modalBlock.classList.remove("active");
}

/**
 * Cierra el modal.
 */
function closeModal() {
    modal.classList.remove("active");
    blurScreen.classList.toggle("blur");
    modalBlock.classList.remove("active");
}

/**
 * Abre o cierra las listas de resultados.
 * @param {HTMLElement} elemento - boton de toggle de las listas de resultados.
 */
function toggleList(elemento) {
    var registrados = document.getElementById("registered");
    var noRegistrados = document.getElementById("no-registered");

    if (elemento.id == "toggle-registered") {
        if (!registrados.classList.contains("active")) {
            registrados.classList.add("active");

            if (noRegistrados.classList.contains("active")) {
                noRegistrados.classList.remove("active");
            }

        } else {
            registrados.classList.remove("active");
        }
    } else if (elemento.id == "toggle-no-registered") {
        if (!noRegistrados.classList.contains("active")) {
            noRegistrados.classList.add("active");

            if (registrados.classList.contains("active")) {
                registrados.classList.remove("active");
            }

        } else {
            noRegistrados.classList.remove("active");
        }
    }
}