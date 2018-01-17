// tab switch function
$('.tab').click(function (e) {
    var $tab = $(this);

    e.preventDefault();

    if (!$tab.parent().hasClass('activeTab')) {
        $(".tabs .activeTab").removeClass('activeTab');
        $tab.parent().addClass('activeTab');
        $('.tabPage').addClass('hidden');
        $('#' + $tab.attr('data-show-page')).removeClass('hidden');
    }
});