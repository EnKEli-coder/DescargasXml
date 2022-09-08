function obtenerMeses(url) {
    var anio = document.getElementById("anios").value;
    axios.post(url, {
        anioSelect: anio
    })
        .then(function (response) {

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
            
        })
        .catch(function (error) {
            console.log(error);
        });
}

function obtenerPartidas(url) {

    var boton = document.getElementById("boton-descarga");
    var carpetas = document.getElementById("orden");
    var spanCarpetas = document.getElementById("span-orden");

    var docOptions = document.getElementsByClassName("doc-check");

    var checkAll = document.getElementById("container-mark-all");
    var lista = document.getElementById("lista-partidas");
    checkAll.style.display = "none";
    lista.style.display = "none";

    var cargando = document.getElementById("carga");
    cargando.style.display = "inline-block";

    var anio = document.getElementById("anios").value;
    axios.post(url, {
        anioSelect: anio
    }).then(function (response) {

        var json = JSON.parse(response.data);

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
            checkAll.style.display = "flex";


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


            for (let opcion of docOptions) {
                if (opcion.hasAttribute("disabled")) {
                    opcion.removeAttribute("disabled");
                    opcion.classList.remove("disabled");
                }
                opcion.checked = false;
            }
            
        }

    }).catch(function (error) {
        console.log(error);
    });
}


function elegirDescarga() {
    var excel = document.getElementById("excelOption");
    var xml = document.getElementById("xmlOption");

    if (excel.checked && xml.checked) {
        descargarXls("/DescargasXml/DescargarXls");
        descargar("/DescargasXml/Descargar");
    } else if (excel.checked) {
        descargarXls("/DescargasXml/DescargarXls");
    } else {
        descargar("/DescargasXml/Descargar");
    }
}

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
            var valor = partida.nextElementSibling.innerHTML;
            lista += valor;
            i++;
        }
    }

    if (mes == 0) {
        nombreDescarga = 'Xmls_' + anio + '.xlsx';
    } else {
        nombreDescarga = 'Xmls_' + nombreMes + '_' + anio + '.xlsx';
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
                console.log(error.response.data);
                $.unblockUI();
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
                console.log(error.response.data);
            });
    }
}


function descargar(url) {

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
            var valor = partida.nextElementSibling.innerHTML;
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

    axios.post(url,
    {
        anio: anio,
        mes: mes,
        partidas: lista,
        carpetas: carpetas,
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
        console.log(error.response.data);
        $.unblockUI();
    });
}
    //const response = await axios.post(url, { anioSelect: anio })
    //console.log(response);

function resume(url, datos) {

   
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
            html: listaAnio + "<br>" + listaMes + "<br>" + listaDocumento + listaOrden  +"<br>"+ listaPartida,
            heightAuto: false,
            customClass: {
                popup: 'popupswal',
            }
        })
    }
    else
    {


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
                var valor = partida.nextElementSibling.innerHTML;
                lista += valor;
                i++;
            }
        }

        var archivosADescargar = ""
        if (excel.checked && xml.checked) {
            archivosADescargar = "Se descargaran XMLs y Excel.";
        } else if (excel.checked) {
            archivosADescargar = "Se descargara un Excel.";
        } else {
            archivosADescargar = "Se descargaran XMLs.";
        }

        axios.post(datos,
            {
                anio: anio,
                mes: mes,
                partidas: lista,
            }).then(function (response) {
                $.unblockUI();
                var json = JSON.parse(response.data)

                var monto = new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(json[1])

                Swal.fire({
                    title: 'Confirmar descarga',
                    html: 'XMLs: ' + json[0] + " archivos.<br>ISR: " + monto + " MXN.<br>"+archivosADescargar,
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
                })
            })
    }

    
}

function activarBoton() {
    var meses = document.getElementById("meses");
    var mes = meses.options[meses.selectedIndex].value;
    var orden = document.getElementById("orden");
    var disposicion = orden.options[orden.selectedIndex].value;
    var boton = document.getElementById("boton-descarga");

    if (mes >= 0 && disposicion >= 0) {
        boton.classList.remove("disabled");
    }
}

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