/*-----------------------------------------------------------------------------------------------------*/
/*Funciones Globales para los Mensajes*/
//Rojo
function MensajeError(Texto) {
    noty({
        text: "<span style='font-size:13px; font-weight: bold;'>" + "  " + Texto + "  " + "</span>",
        type: 'error',
        layout: 'topCenter',
        dismissQueue: false,        
        modal: true
    });
}
//Verde
function MensajeExito(Texto) {
    noty({
        text: "<span style='font-size:13px; font-weight: bold;'>" + "  " + Texto + "  " + "</span>",
        type: 'success',
        layout: 'topCenter',
        dismissQueue: false,
        modal: true
    });
}
//Azul
function MensajeInformacion(Texto) {
    noty({
        text: "<span style='font-size:13px; font-weight: bold;'>" + "  " + Texto + "  " + "</span>",
        type: 'information',
        layout: 'topCenter',
        dismissQueue: false,
        modal: true
    });
}
//Blanco
function MensajeAlerta(Texto)
{
    noty({
        text: "<span style='font-size:13px; font-weight: bold;'>" + "  " + Texto + "  " + "</span>",
        type: 'alert',
        layout: 'topCenter',
        dismissQueue: false,
        modal: true
    });
}
//Amarillo
function MensajeAdvertencia(Texto)
{
    noty({
        text: "<span style='font-size:13px; font-weight: bold;'>" + "  " + Texto + "  " + "</span>",
        type: 'warning',
        layout: 'topCenter',
        dismissQueue: false,
        modal: true
    });
}
//usar este metodo para detener el envio del servidor si se usa el de abajo el plugin del noti enviara al servidor aunque
//tenga el return false
function Confirmacion(Cadena, s) {

    MensajeConfirmar(Cadena, s);
    return false;
}
function MensajeConfirmar(Texto, IdBtn) {
    noty({
        text: "<span style='font-size:15px; font-weight: bold;'>" + "  " + Texto + "  " + "</span>",
        type: 'Confirm',
        layout: 'center',
        dismissQueue: false,
        modal: true,        
        buttons: [{
            addClass: 'btn btn-success',
            text: "<span style='font-size:14px; font-weight: bold;'>" + 'Si' + "</span>",
            onClick: function ($noty) {                
                
                $noty.close();
                var name = IdBtn.replace(/_/g, "$");
                __doPostBack(name, "");                
            }
        },
        {
            addClass: 'btn  btn-danger',
            text: "<span style='font-size:14px; font-weight: bold;'>" + 'No' + "</span>",
            onClick: function ($noty) { 
                $noty.close();               
            }
        }]
    });
    return false;
}
function MensajeConfirmarDVE(Texto, sender, event) {
    noty({
        text: "<span style='font-size:13px; font-weight: bold;'>" + "  " + Texto + "  " + "</span>",
        type: 'Confirm',
        layout: 'center',
        dismissQueue: false,
        modal: true,
        modal: true,
        buttons: [{
            addClass: 'btn btn-primary',
            text: "<span style='font-size:14px; font-weight: bold;'>" + 'Si' + "</span>",
            onClick: function ($noty) {
                $noty.close();
                ejecutar = true;
                sender.DoClick();
            }
        },
        {
            addClass: 'btn btn-danger',
            text: "<span style='font-size:14px; font-weight: bold;'>" + 'No' + "</span>",
            onClick: function ($noty) {
                $noty.close();
                ejecutar = false;
                sender.DoClick();
            }
        }]
    });
}

function MensajeConfirmar(Texto, Funcion) {
    noty({
        text: "<span style='font-size:15px; font-weight: bold;'>" + "  " + Texto + "  " + "</span>",
        type: 'Confirm',
        layout: 'center',
        dismissQueue: false,
        modal: true,
        buttons: [{
            addClass: 'btn btn-success',
            text: "<span style='font-size:14px; font-weight: bold;'>" + 'Si' + "</span>",
            onClick: function ($noty) {
                $noty.close();
                Funcion();
            }
        },
        {
            addClass: 'btn  btn-danger',
            text: "<span style='font-size:14px; font-weight: bold;'>" + 'No' + "</span>",
            onClick: function ($noty) {
                $noty.close();
            }
        }]
    });
    return false;
}


//MensajeCargando
function MensajeCargando() {
    $.blockUI({
        message: `
                    <h3 style="font-size:1.2rem;">Espere un momento</h3>
                    <div class='sk-three-bounce'><div class='sk-child sk-bounce1'></div><div class='sk-child sk-bounce2'></div><div class='sk-child sk-bounce3'></div></div>
                `,
        css: {
            border: 'none',
            padding: '15px',
            backgroundColor: '#000',
            '-webkit-border-radius': '10px',
            '-moz-border-radius': '10px',
            opacity: .5,
            color: '#fff'
        }
    });


}

function OcultarMensajeCargando() {
    $.unblockUI();
}






/**********************************************************************************************************************************************************************************************************************************/
/**********************************************************************************************************************************************************************************************************************************/
/***********************************************************************************       Mensajes Con Sweet alert V9.X        ***************************************************************************************************/
/**********************************************************************************************************************************************************************************************************************************/
/**********************************************************************************************************************************************************************************************************************************/


function MensajeErrorSweet(Solucion, Error)
{
    Swal.fire({
        backdrop: true,
        allowEnterKey: false,
        allowOutsideClick: false,
        allowEscapeKey: false,
        icon: 'error',
        title:  Error ,
        text: Solucion,
        footer: '<a href="#">Contactar al desarrollador?</a>'
    });
}


function MensajeWarningSweet(Texto)
{
    Swal.fire({
        backdrop: true,
        allowEnterKey: false,
        allowOutsideClick: false,
        allowEscapeKey: false,
        icon: 'warning',
        title: '',
        text: Texto,
        footer: '<a href="#">Contactar al desarrollador?</a>'
    });


}



function MensajeCorrectoSweet(Texto)
{
    Swal.fire({
        backdrop: true,
        allowEnterKey: false,
        allowOutsideClick: false,
        allowEscapeKey: false,
        icon: 'success',
        title: '',
        text: Texto,
        footer: '<a href="#">Contactar al desarrollador?</a>'
    });


}



function MensajeCorrecto_sinClickSweet(Texto) {
    Swal.fire({
        backdrop: true,
        allowEnterKey: true,
        allowOutsideClick: true,
        allowEscapeKey: true,
        icon: 'success',
        title: '',
        text: Texto,
        footer: '<a href="#">Contactar al desarrollador?</a>'
    });


}



function RecargarSoloPaginaActual()
{
    window.location.reload();
}




function MensajeCorrectoConRecargaPagina(Texto)
{
    Swal.fire({
     
        title: '',
        text: Texto,
        icon: 'success',
        showCancelButton: false,
        allowOutsideClick: false,
        confirmButtonColor: '#3085d6',
        confirmButtonText: 'OK',
        footer: '<a href="#">Contactar al desarrollador?</a>'
    }).then((result) => {
        if (result.isConfirmed) {
            window.location.reload(); 
        }
    })
}




function MensajeCorrectoConRecargaPagina_UnicaParaRecuperFoliosEnVistaDetalle(Texto) {

    Swal.fire({
        title: '',
        text: Texto,
        icon: 'success',
        showCancelButton: false,
        allowOutsideClick: false,
        confirmButtonColor: '#3085d6',
        confirmButtonText: 'OK',
        footer: '<a href="#">Contactar al desarrollador?</a>'
    }).then((result) => {
        if (result.isConfirmed) {

           // $("#table_id").empty();
            CargarDetallesFolios();
        }
    })
}










function MensajeInformacionSweet(Titulo, Texto)
{
    Swal.fire({
        backdrop: true,
        allowEnterKey: true,
        allowOutsideClick: true,
        allowEscapeKey: true,
        icon: 'info',
        title: Titulo,
        text: Texto,
        footer: '<a href="#">Contactar al desarrollador?</a>'
    });

}



function MensajeGuardar_NoGuardar_Cancelar( TextoAguardar , ConfirmacionDeGuardado , ConfirmacionDeNoSeraGuardado )
{
    Swal.fire({
        title: TextoAguardar/*'Do you want to save the changes?'*/,
        text: 'Something went wrong!',
        showDenyButton: true,
        showCancelButton: true,
        confirmButtonText: 'Save',
        denyButtonText: `Don't save`,
    }).then((result) => {
        /* Read more about isConfirmed, isDenied below */
        if (result.isConfirmed) {








            MensajeCorrectoSweet(ConfirmacionDeGuardado);
            /*Swal.fire('Saved!', '', 'success')*/
        } else if (result.isDenied) {

            MensajeInformacionSweet(ConfirmacionDeNoSeraGuardado)
           /* Swal.fire('Changes are not saved', '', 'info')*/
        }
    })

}




function MensajeEstamosEnConstrucion()
{
    Swal.fire({
        title: '<strong>  "PROXIMAMENTE DISPONIBLE"</strong>',
        icon: 'info',
        html:'<b> Estamos trabajando en este nuevo recurso, Disculpe las molestias. </b>', 
        showCloseButton: true,
        showCancelButton: true,
        focusConfirm: false,
        confirmButtonText:
            '<i class="fa fa-thumbs-up"></i> Genial!',
        confirmButtonAriaLabel: 'Thumbs up, great!',
        cancelButtonText:
            '<i class="fa fa-thumbs-down"></i>',
        cancelButtonAriaLabel: 'Thumbs down'
    })

}





/*Un cuadro de diálogo de confirmación, con una función adjunta al botón "Confirmar"*/
function MensajeConFuncionAdjuntaDeConfirmar( PreguntaSeguridad , Texto )
{



    Swal.fire({
        title: PreguntaSeguridad,
        text: Texto,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'No, cancelar!',
        footer: '<a href="#">Contactar al desarrollador?</a>'
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.fire(
                'Deleted!',
                'Your file has been deleted.',
                'success'
            )
        }
    })

    //Se usa en Solicitar.js Metodo => CrearSolicitudFormasPago()
}












