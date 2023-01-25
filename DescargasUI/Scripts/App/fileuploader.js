
var uploadObj;
var files = [];


$(document).ready(function () {
	uploadObj = $("#fileuploader").uploadFile({
		url: "/subirXmls/GuardarRegistro",
		fileName: "file",
		acceptFiles: "application/xml",
		allowedTypes: "xml",
		dragDropStr: "<span><b>Arrastra y suelta los archivos.</b></span>",
		uploadStr: "Subir archivos",
		showQueueDiv: "queue-container",
		autoSubmit: false,
		serialize: false,
		multiple: true,
		dragDrop: true,
		onSelect: function (files) {
			for (var file of files) {
				fileName = file.name;
            }
        }
	});

});

function Subir() {
	uploadObj.startUpload();
}