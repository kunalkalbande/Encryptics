$(function () {
    $('.hideMessage').click(function (e) {
        e.preventDefault();
        $(this).closest('.ui-widget').hide("highlight", {}, 600);
    });
});