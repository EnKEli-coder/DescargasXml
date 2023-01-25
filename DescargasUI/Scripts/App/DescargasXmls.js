/**
 *Controla la vista de Descargas de archivos del Poder Ejecutivo.
 * 
 *Variables globales:
 *      - listaUniverso: Objeto Json, lista de ramos y partida que forman el universo.
 *      - delayTimer: int, tiempo de retraso para escribir en el buscador.
 *      - modal: HTMLElement, div del modal.
 *      - modalBlock: HTMLElement, div que bloquea la pagina cuando se abre el modal.
 */
var listaUniverso;
var delayTimer;
var modal = document.getElementById("modal");
var modalBlock = document.getElementById("modal-block");

$(document).ready(function () {
    var barraBusqueda = document.getElementById("input-busqueda");
    barraBusqueda.addEventListener('input', busqueda);
    var botonBuscar = document.getElementById("boton-busqueda");
    botonBuscar.addEventListener('click', busqueda);
})

/**
 * Obtiene los meses de acuerdo al año que se seleccione y rellena el select de meses.
 * @param {string} url - ruta al controlador ~/DescargasXmlController/DescargasXmls(int anio).
 */
function obtenerMeses(url) {
    var anio = document.getElementById("anios").value;
    axios.post(url,
        {
            anioSelect: anio
        },
        {
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
        }
   ).then(function (response) {

            var json = JSON.parse(response.data)

            $('#meses option').remove();

            var select = document.getElementById('meses');
            var spanMeses = document.getElementById("span-meses");

            var hidden = document.createElement("option");
            hidden.value = "none";
            hidden.text = "Escoge un mes";
            hidden.setAttribute("selected", "");
            hidden.setAttribute("hidden", "");
            hidden.setAttribute("disabled", "");
            select.add(hidden);

            var todos = document.createElement("option");
            todos.value = "0";
            todos.text = "Todos los meses";
            select.add(todos);

            for (var value in json) {
                var option = document.createElement("option");
                option.value = value;
                option.text = json[value];
                select.add(option);
            };


            if (select.hasAttribute("disabled")) {
                select.removeAttribute("disabled");
                spanMeses.classList.remove("disabled");
            }
            
        }).catch(function (error) {
            openModal("Error", "Ha ocurrido un error, intente de nuevo mas tarde.", "closeModal()");
        });
}

/**
 * Obtiene las partidas de acuerdo al año que se seleccione y rellena la lista de ramos y partidas.
 * @param {string} url - ruta al controlador ~/DescargasXmlController/ListaPartidas(int anio).
 */
function obtenerPartidas(url) {

    var buscar = document.getElementById("input-busqueda");
    var botonBuscar = document.getElementById("boton-busqueda");
    var todos = document.getElementById("check-all");
    var boton = document.getElementById("boton-descarga");
    var carpetas = document.getElementById("orden");
    var spanCarpetas = document.getElementById("span-orden");

    var docOptions = document.getElementsByClassName("doc-check");

    //var checkAll = document.getElementById("container-mark-all");
    var lista = document.getElementById("lista-partidas");
   // checkAll.style.display = "none";
    lista.style.display = "none";
    buscar.setAttribute("disabled", "");
    botonBuscar.setAttribute("disabled", "");

    var cargando = document.getElementById("carga");
    cargando.style.display = "inline-block";

    var anio = document.getElementById("anios").value;

    axios.post(url, 
        {
            anioSelect: anio
        },
        {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
    }).then(function (response) {

        var json = JSON.parse(response.data);

        listaUniverso = json;

        $('#lista-partidas li').remove();

        for (var unidad in json) {
            var li = document.createElement("li");
            var input = document.createElement("input")
            var label = document.createElement("label")
            var i = document.createElement("i");


            li.className = "parent-list-item";
            input.type = "checkbox";
            input.className = "ramos";
            input.setAttribute("onclick", "checkear(this)");
            label.innerHTML = unidad;
            i.className = "fa-solid fa-plus";
            i.setAttribute("onclick", "activar(this)");
            li.appendChild(i);
            li.appendChild(input);
            li.appendChild(label);

            lista.insertAdjacentElement('beforeend', li);
            var ul = document.createElement("ul");


            for (var partida in json[unidad]) {
                
                var liChild = document.createElement("li");
                var inputChild = document.createElement("input")
                var labelChild = document.createElement("label")

                ul.className = "xml-list";
                liChild.className = "check";
                inputChild.type = "checkbox";
                inputChild.className = "seleccion";
                inputChild.setAttribute("onclick", "seleccionar(this)");
                labelChild.innerHTML = json[unidad][partida];
                liChild.appendChild(inputChild);
                liChild.appendChild(labelChild);
                ul.insertAdjacentElement('beforeend', liChild);
            }
            li.insertAdjacentElement("afterend", ul);

            cargando.style.display = "none";
            lista.style.display = "block";
            //checkAll.style.display = "flex";


            boton.classList.add("disabled");

            var hidden = document.createElement("option");
            hidden.value = "none";
            hidden.text = "Escoge una opcion";
            hidden.setAttribute("selected", "");
            hidden.setAttribute("hidden", "");
            hidden.setAttribute("disabled", "");
            carpetas.add(hidden);

            carpetas.setAttribute("disabled", "");
            spanCarpetas.classList.add("disabled");
            buscar.removeAttribute("disabled", "");
            botonBuscar.removeAttribute("disabled", "");
            todos.checked = false;

            for (let opcion of docOptions) {
                if (opcion.hasAttribute("disabled")) {
                    opcion.removeAttribute("disabled");
                    opcion.classList.remove("disabled");
                }
                opcion.checked = false;
            }
            
        }

    }).catch(function (error) {
        openModal("Error", "Ha ocurrido un error, intente de nuevo mas tarde.", "closeModal()");
    });
}

/** 
 *  Filtra la lista de ramos y partidas cuando el usuario escribe en el buscador.
 */
function busqueda() {

    clearTimeout(delayTimer);
    delayTimer = setTimeout(function () {
        var buscar = document.getElementById("input-busqueda");
        var botonBuscar = document.getElementById("boton-busqueda");
        var busqueda = buscar.value.normalize("NFD").replace(/[\u0300-\u036f]/g, "");;
        var noResult = document.getElementById("no-result");
        var lista = document.getElementById("lista-partidas");
        var cargando = document.getElementById("carga");
        var todos = document.getElementById("check-all");

        //buscar.setAttribute("disabled", "");
        //botonBuscar.setAttribute("disabled", "");
        lista.style.display = "none";
        noResult.style.display = "none"
        cargando.style.display = "inline-block";

        var listaFiltrada = {};

        for (var unidad in listaUniverso) {
            var partidasFiltradas = [];
            partidasFiltradas = listaUniverso[unidad].filter(partida => partida.toLowerCase().includes(busqueda.toLowerCase()));
            if (partidasFiltradas.length > 0) {
                listaFiltrada[unidad] = partidasFiltradas;
            }
        }

        if (Object.keys(listaFiltrada).length > 0) {

            $('#lista-partidas li').remove();

            for (var unidad in listaFiltrada) {
                var li = document.createElement("li");
                var input = document.createElement("input")
                var label = document.createElement("label")
                var i = document.createElement("i");


                li.className = "parent-list-item";
                input.type = "checkbox";
                input.className = "ramos";
                input.setAttribute("onclick", "checkear(this)");
                label.innerHTML = unidad;
                i.className = "fa-solid fa-plus";
                i.setAttribute("onclick", "activar(this)");
                li.appendChild(i);
                li.appendChild(input);
                li.appendChild(label);

                lista.insertAdjacentElement('beforeend', li);
                var ul = document.createElement("ul");


                for (var partida in listaFiltrada[unidad]) {

                    var liChild = document.createElement("li");
                    var inputChild = document.createElement("input")
                    var labelChild = document.createElement("label")

                    ul.className = "xml-list";
                    liChild.className = "check";
                    inputChild.type = "checkbox";
                    inputChild.className = "seleccion";
                    inputChild.setAttribute("onclick", "seleccionar(this)");
                    labelChild.innerHTML = listaFiltrada[unidad][partida];
                    liChild.appendChild(inputChild);
                    liChild.appendChild(labelChild);
                    ul.insertAdjacentElement('beforeend', liChild);
                }
                li.insertAdjacentElement("afterend", ul);

                cargando.style.display = "none";
                lista.style.display = "block";
                botonBuscar.removeAttribute("disabled", "");
                buscar.removeAttribute("disabled", "");
                buscar.focus();
                todos.checked = false;
            }
        } else {
            cargando.style.display = "none";
            noResult.style.display = "inline-block";
            //buscar.removeAttribute("disabled", "");
            //botonBuscar.removeAttribute("disabled", "");
            //buscar.focus();
            todos.checked = false;
        }
    }, 100)
}

/**
 * Muestra en un modal, la cantidad de archivos a descargar, el isr total de los Xml, los
 * archivos que se descargaran: xmls, xls o ambos, y un boton de confirmacion. Se muestra 
 * al dar click al boton de Descargar.
 * @param {string} datos - ruta al controlador ~/DescargasXmlController/Datos(int anio, int mes, string partidas).
 */
function resume(datos) {

    var anios = document.getElementById("anios");
    var anioOption = anios.options[anios.selectedIndex].value;
    var meses = document.getElementById("meses");
    var mesOption = meses.options[meses.selectedIndex].value;
    var orden = document.getElementById("orden");
    var disposicion = orden.options[orden.selectedIndex].value;
    var excel = document.getElementById("excelOption");
    var xml = document.getElementById("xmlOption");
    var ramos = document.getElementsByClassName("ramos");
    var seleccionados = 0;

    for (let ramo of ramos) {
        if (ramo.checked) {
            seleccionados++;
        }
    }

    if (!((anioOption >= 0 && mesOption >= 0 && seleccionados > 0) && ((xml.checked && disposicion >= 0) || excel.checked))) {

        var iconAnio = anioOption >= 0 ? 'fa-check' : 'fa-xmark';
        var iconMes = mesOption >= 1 ? 'fa-check' : 'fa-xmark';
        if (xml.checked) {
            var iconOrden = disposicion >= 0 ? 'fa-check' : 'fa-xmark';
        }
        var iconDocumento = excel.checked || xml.checked ? 'fa-check' : 'fa-xmark';
        var iconPartidas = seleccionados > 0 ? 'fa-check' : 'fa-xmark';

        var listaAnio = '<i id="carga" class="fa-solid ' + iconAnio + '"></i>Año seleccionado';
        var listaMes = '<i id="carga" class="fa-solid ' + iconMes + '"></i>Mes seleccionado';
        if (xml.checked) {
            var listaOrden = '<br><i id="carga" class="fa-solid ' + iconOrden + '"></i>Disposición seleccionada';
        } else {
            var listaOrden = ""
        }

        var listaDocumento = '<i id="carga" class="fa-solid ' + iconDocumento + '"></i>Seleccionar archivos';
        var listaPartida = '<i id="carga" class="fa-solid ' + iconPartidas + '"></i>Partidas seleccionadas';

        Swal.fire({
            icon: 'warning',
            iconColor: '#A02141',
            title: 'Selecciona la información faltante',
            html: listaAnio + "<br>" + listaMes + "<br>" + listaDocumento + listaOrden + "<br>" + listaPartida,
            heightAuto: false,
            customClass: {
                popup: 'popupswal',
            }
        })
    }
    else {


        $.blockUI({
            message: '<h1>Recopilando información...</h1>',
            css: {
                backgroundColor: '#A02141',
                color: '#fff',
                border: 'none',
                borderRadius: '8px'
            }
        });

        var anio = anios.value;
        var mes = meses.value;
        var partidas = document.getElementsByClassName("seleccion");
        var lista = "";
        var i = 0;

        for (let partida of partidas) {
            if (partida.checked) {

                if (i != 0) {
                    lista += ",";
                }
                var valor = partida.nextElementSibling.innerHTML.substring(0, 6);
                lista += valor;
                i++;
            }
        }

        var archivosADescargar = ""
        if (excel.checked && xml.checked) {
            archivosADescargar = "Se descargarán XMLs y Excel.";
        } else if (excel.checked) {
            archivosADescargar = "Se descargará un Excel.";
        } else {
            archivosADescargar = "Se descargaran XMLs.";
        }

        axios.post(datos,
        {
            anio: anio,
            mes: mes,
            partidas: lista
        }, {
             headers: { 'X-Requested-With': 'XMLHttpRequest' }
        }).then(function (response) {
            $.unblockUI();
            var json = JSON.parse(response.data)

            var monto = new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(json[1])

            Swal.fire({
                title: 'Confirmar descarga',
                html: 'XMLs: ' + json[0] + " archivos.<br>ISR: " + monto + " MXN.<br>" + archivosADescargar,
                icon: 'question',
                iconColor: '#A02141',
                confirmButtonText: 'Descargar',
                showCancelButton: 'true',
                cancelButtonText: 'Cancelar',
                heightAuto: false,
                customClass: {
                    popup: 'popupswal',
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    elegirDescarga();
                }
            }).catch(function (error) {
                $.unblockUI();
                openModal("Error", "Ha ocurrido un error, intente de nuevo mas tarde.", "closeModal()");
            });
        })
    }


}

/**
 * Ejecuta las funciones de descargarXls y descargarXml, de acuerdo a los archivos que el
 * usuario eligiera para la descarga.
 */
function elegirDescarga() {
    var excel = document.getElementById("excelOption");
    var xml = document.getElementById("xmlOption");

    if (excel.checked && xml.checked) {
        descargarXls("/DescargasXml/DescargarXls");
        descargarXml("/DescargasXml/DescargarXml");
    } else if (excel.checked) {
        descargarXls("/DescargasXml/DescargarXls");
    } else {
        descargarXml("/DescargasXml/DescargarXml");
    }
}

/**
 * Descarga un archivo Xls con los datos de los archivos de los ramos y partidas seleccionadas.
 * @param {string} url - ruta al controlador ~/DescargasXmlController/DescargarXls(int anio, int mes, string partidas).
 */
function descargarXls(url) {

    var xml = document.getElementById("xmlOption");
    var anio = document.getElementById("anios").value;
    var select = document.getElementById("meses");
    var mes = select.value;
    var nombreMes = select.options[select.selectedIndex].text.toLowerCase();
    var partidas = document.getElementsByClassName("seleccion");
    var lista = "";
    var i = 0;

    for (let partida of partidas) {
        if (partida.checked) {

            if (i != 0) {
                lista += ",";
            }
            var valor = partida.nextElementSibling.innerHTML.substring(0,6);
            lista += valor;
            i++;
        }
    }

    if (mes == 0) {
        nombreDescarga = 'Reportes_' + anio + '.zip';
    } else {
        nombreDescarga = 'Reportes_' + nombreMes + '_' + anio + '.zip';
    }

    if (!xml.checked) {
        $.blockUI({
            message: '<h1>Generando archivo...</h1>',
            css: {
                backgroundColor: '#A02141',
                color: '#fff',
                border: 'none',
                borderRadius: '8px'
            }
        });

        axios.post(url,
        {
            anio: anio,
            mes: mes,
            partidas: lista,
        },{
            responseType: 'arraybuffer',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        }).then(function (response) {
            $.unblockUI();
            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', nombreDescarga);
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            Swal.fire({
                icon: 'success',
                title: 'Descarga completada',
                confirmButtonText: 'Aceptar',
                heightAuto: false,
                customClass: {
                    popup: 'popupswal',
                }
            })

            obtenerMeses("/DescargasXml/DescargasXml");
            obtenerPartidas('/DescargasXml/ListaPartidas');

        }).catch(function (error) {
            $.unblockUI();
            openModal("Error", "Ha ocurrido un error, intente de nuevo mas tarde.", "closeModal()");
        });
    } else {
        axios.post(url,
            {
                anio: anio,
                mes: mes,
                partidas: lista,
            },
            {
                responseType: 'arraybuffer'
            }).then(function (response) {
                $.unblockUI();
                const url = window.URL.createObjectURL(new Blob([response.data]));
                const link = document.createElement('a');
                link.href = url;
                link.setAttribute('download', nombreDescarga);
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);

            }).catch(function (error) {
                $.unblockUI();
                openModal("Error", "Ha ocurrido un error, intente de nuevo mas tarde.", "closeModal()");
            });
    }
}

/**
 * Descarga un archivo comprimido con los xml de los ramos y partidas seleccionadas.
 * @param {string} url - ruta al controlador ~/DescargasXmlController/DescargarXml(int anio, int mes, string partidas, int carpetas).
 */
function descargarXml(url) {

    var nombreCarpeta;
    var nombreDescarga;
    var anio = document.getElementById("anios").value;
    var select = document.getElementById("meses");
    var mes = select.value;
    var nombreMes = select.options[select.selectedIndex].text.toLowerCase();
    var carpetas = document.getElementById("orden").value;
    var partidas = document.getElementsByClassName("seleccion");
    var ramos = document.getElementsByClassName("ramos");
    var lista = "";
    var seleccionados = 0;
    var i = 0;

    for (let ramo of ramos) {
        if (ramo.checked) {
            seleccionados++;
            if (seleccionados == 1) {
                nombreCarpeta = ramo.nextElementSibling.innerHTML;
            }
        }
    }

    if (seleccionados > 1) {
        nombreCarpeta = "XMLs";
    }


    if (mes == 0) {
        nombreDescarga = nombreCarpeta + '_' + anio + '.zip';
    } else {
        nombreDescarga = nombreCarpeta + '_' + nombreMes + '_' + anio + '.zip';
    }

    for (let partida of partidas) {
        if (partida.checked) {

            if (i != 0) {
                lista += ",";
            }
            var valor = partida.nextElementSibling.innerHTML.substring(0,6);
            lista += valor;
            i++;
        }
    }
    $.blockUI({
        message: '<h1>Comprimiendo archivos...</h1>',
        css: {
            backgroundColor: '#A02141',
            color: '#fff',
            border: 'none',
            borderRadius: '8px'
        }
    });

    axios.post(url, {
        anio: anio,
        mes: mes,
        partidas: lista,
        carpetas: carpetas,
    }, {
        responseType: 'arraybuffer',
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    }).then(function (response) {
        $.unblockUI();
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', nombreDescarga);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        Swal.fire({
            icon: 'success',
            title: 'Descarga completada',
            confirmButtonText: 'Aceptar',
            heightAuto: false,
            customClass: {
                popup: 'popupswal',
            }
        })

        obtenerMeses("/DescargasXml/DescargasXml");
        obtenerPartidas('/DescargasXml/ListaPartidas');
        
    }).catch(function (error) {
        $.unblockUI();
        openModal("Error", "Ha ocurrido un error, intente de nuevo mas tarde.", "closeModal()");
    });
}

/**Activa el boton de Descargar, si todas las opciones necesarias estan completas.*/
function activarBoton() {
    var meses = document.getElementById("meses");
    var mes = meses.options[meses.selectedIndex].value;
    var orden = document.getElementById("orden");
    var disposicion = orden.options[orden.selectedIndex].value;
    var boton = document.getElementById("boton-descarga");
    var excel = document.getElementById("excelOption");
    var xml = document.getElementById("xmlOption");


    if (mes >= 0 && ((xml.checked && disposicion >= 0) || excel.checked)) {
        boton.classList.remove("disabled");
    } else {
        boton.classList.add("disabled");
    }
}

/**Activa el Select de Estructura cuando se selecciona la descarga de xml.*/
function activarCarpetas() {
    var xml = document.getElementById("xmlOption");
    var carpetas = document.getElementById("orden");
    var spanCarpetas = document.getElementById("span-orden");

    if (xml.checked) {
        carpetas.removeAttribute("disabled");
        spanCarpetas.classList.remove("disabled");
    } else {
        carpetas.setAttribute("disabled", "");
        spanCarpetas.classList.add("disabled");
    }
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
}

/**Cierra el modal*/
function closeModal() {
    modal.classList.remove("active");
    modalBlock.classList.remove("active");
}