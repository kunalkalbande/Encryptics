$.widget("custom.catcomplete", $.ui.autocomplete, {
    _create: function () {
        this._super();
        this.widget().menu("option", "items", "> :not(.ui-autocomplete-category)");
    },
    _renderMenu: function (ul, items) {
        var that = this,
            currentCategory = "";
        $.each(items, function (index, item) {
            var li;
            if (item.category != currentCategory) {
                ul.append("<li class='ui-autocomplete-category'>" + item.category + "</li>");
                currentCategory = item.category;
            }
            li = that._renderItemData(ul, item);
            if (item.category) {
                li.attr("aria-label", item.category + " : " + item.label);
            }
        });
    }
});

$(function () {
    $('#siteSearchTerm').catcomplete({
        source: getSiteSearchReslts,
        minLength: 2,
        focus: changeSiteSelection,
        select: changeSiteSelection
    });
    
    $("#siteSearchButton").button({
        icons: {
            primary: "ui-icon-search"
        },
        text: false
    });
    
    $('#siteSearchTerm').keydown(function (e) {
        if (e.keyCode == 13 && processKeyDown) {
            $('#siteSearchForm').submit();
        }
    });
    
    $('#siteSearchButton').click(function () {
        $('#siteSearchForm').submit();
    });
});

var processKeyDown = true; // kludge :(

function changeSiteSelection(event, ui) {
    event.preventDefault();

    if (event.type == 'catcompleteselect') {
        processKeyDown = false;
        window.location = ui.item.value;
    }
}

function getSiteSearchReslts(request, response) {
    $('#siteSearchTerm')
        .css('background', 'white url(/Images/tinyloader1.gif) no-repeat 288px 4px')
        //.css('opacity', '0.5')
        //.prop('disabled', true)
    ;
    
    ajaxGetSetup(searchUrl, function (token) {
        ajaxPostForm(searchUrl, token, 'searchTerm=' + request.term, function (data) {
            ajaxResponseReturned(data, function (responseData) {
                //console.debug(responseData);
                $('#siteSearchTerm').prop('disabled', false).css('background', '')/*.css('opacity', '')*/;
                response(responseData);
            });
        });
    });
}