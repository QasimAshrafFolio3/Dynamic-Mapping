// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//Print json with format.
function PrettyPrintJson(jsObj) {
    $("#jsonataResult").text(jsObj, null, "\t"); // stringify with tabs inserted at each level
}