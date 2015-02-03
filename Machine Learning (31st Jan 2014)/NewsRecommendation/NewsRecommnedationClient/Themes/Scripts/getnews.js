
$("#recommendation_panel").hide();

var client = new WindowsAzure.MobileServiceClient(
    "https://newsrecommendation.azure-mobile.net/",
    "hilNtOcXpPfEosRNEnXrysCaVognsD51"
);


client.invokeApi("GetNews", {
        body: null,       
        method: "get"
    }).done(function (results) {      
        showNews(results.result);
    }, function(error) {
        //error handling...
    });

//Remove SEO Tag
var rawTitle = $(document).attr('title').split("#.")[0];
rawTitle = $.trim(rawTitle.split(" - ")[1]);

function showRecommendations(items) {
    if (items == null) {
        return;
    }

    if (items.length <= 0) {
        return;
    }


    $("#recommendation_panel").show();
    var lst = $("#recommendation_body").append('<div class="list-group"/>');

    lst.empty();

    items.forEach(function(r, i, a) {
        lst.append('<a href="#' + r.url + '" onClick="javascript:getNewsItem(\'' + r.title + '\');">' + r.title + '</a></br>');
    });

    $("#recommendation_header").empty();
    $("#recommendation_header").append("Recommended Posts");
    $("#recommendation_footer").empty();
    $("#recommendation_footer").append("Powered by Azure Machine Learning");
}

function showNews(items) {
    if (items == null) {
        return;
    }
    if (items.length <= 0) {
        return;
    };
    $(".content-items").show();

    var lst = $(".content-items");

    $(".content-items").innerHTML = "";

    items.forEach(function (r, i, a) {
        lst.append('<li> <article class="blog-post"> <div class="published">' + r.pubDate + '</div> <h1><a href="#'+r.url+ '" onClick="javascript:getNewsItem(\'' + r.title + '\');">' + r.title + '</a></h1><p>' + r.text +
            '<a href="#' + r.url + '" onClick="javascript:getNewsItem(\'' + r.title + '\');"> more... </a> </p></article></li>');
    });
   
}

function showNewsItem(item) {
    if (item == null) {
        return;
    }

    var lst = $(".content-items");

    lst.empty();

    lst.append('<li> <article class="blog-post"> <div class="published">' + item.pubDate + '</div> <h1><a href="#' + item.url + '" onClick="javascript:getNewsItem(\'' + item.title + '\');">' + item.title + '</a></h1><p>' + item.text + '</p></article></li>');

    client.invokeApi("BlogRecommendations", {
        body: null,
        parameters: { title: item.title },
        method: "get"
    }).done(function (results) {
        showRecommendations(results.result);
    }, function (error) {
        //error handling...
    });
}

function getNewsItem(newstitle) {
    if (newstitle == null) {
        return;
    }
    if (newstitle.length<=0) {
        return;
    }
    client.invokeApi("GetNews", {
        body: null,
        method: "get",
        parameters: { title: newstitle }
    }).done(function (results) {
        showNewsItem(results.result);
    }, function (error) {
        //error handling...
    });
}


