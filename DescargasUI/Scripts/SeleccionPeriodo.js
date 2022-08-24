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

    var lista = document.getElementById("lista-partidas");
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


            boton.classList.add("disabled");

            var hidden = document.createElement("option");
            hidden.value = "none";
            hidden.text = "Escoge una opcion";
            hidden.setAttribute("selected", "");
            hidden.setAttribute("hidden", "");
            hidden.setAttribute("disabled", "");
            carpetas.add(hidden);

            if (carpetas.hasAttribute("disabled")) {
                carpetas.removeAttribute("disabled");
                spanCarpetas.classList.remove("disabled");
            }
            
        }

    }).catch(function (error) {
        console.log(error);
    });
}

function descargar(url) {

    var nombreCarpeta;
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
        carpetas: carpetas
    },
    {
        responseType: 'arraybuffer'
    }).then(function (response) {
        $.unblockUI();
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', nombreCarpeta+'_' + nombreMes+'_'+anio+'.zip');
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
        //    .then(() => {
            
        //})

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
    var ramos = document.getElementsByClassName("ramos");
    var seleccionados = 0;

    for (let ramo of ramos) {
        if (ramo.checked) {
            seleccionados++;
        }
    }

    if (!(anioOption >= 0 && mesOption >= 1 && disposicion >= 0 && seleccionados > 0)) {

        var iconAnio = anioOption >= 0 ? 'fa-check' : 'fa-xmark';
        var iconMes = mesOption >= 1 ? 'fa-check' : 'fa-xmark';
        var iconOrden = disposicion >= 0 ? 'fa-check' : 'fa-xmark';
        var iconPartidas = seleccionados > 0 ? 'fa-check' : 'fa-xmark';

        var listaAnio = '<i id="carga" class="fa-solid ' + iconAnio + '"></i>Año seleccionado';
        var listaMes = '<i id="carga" class="fa-solid ' + iconMes + '"></i>Mes seleccionado';
        var listaOrden = '<i id="carga" class="fa-solid ' + iconOrden + '"></i>Disposición seleccionada';
        var listaPartida = '<i id="carga" class="fa-solid ' + iconPartidas + '"></i>Partidas seleccionadas';

        Swal.fire({
            icon: 'warning',
            iconColor: '#A02141',
            title: 'Selecciona la información faltante',
            html: listaAnio + "<br>" + listaMes + "<br>" + listaOrden + "<br>" + listaPartida,
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
                    html: 'XMLs: ' + json[0] + " archivos.<br>ISR: " + monto + " MXN.",
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
                        descargar(url)
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

    if (mes >= 1 && disposicion >= 0) {
        boton.classList.remove("disabled");
    }
}