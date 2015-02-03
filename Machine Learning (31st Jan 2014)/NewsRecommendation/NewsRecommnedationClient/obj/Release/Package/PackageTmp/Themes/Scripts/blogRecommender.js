
$("#recommendation_panel").hide();

var client = new WindowsAzure.MobileServiceClient(
    "https://newsrecommendation.azure-mobile.net/",
    "hilNtOcXpPfEosRNEnXrysCaVognsD51"
);

//Remove SEO Tag
var rawTitle = $(document).attr('title').split("#.")[0];
rawTitle = $.trim(rawTitle.split(" - ")[1]);

client.invokeApi("BlogRecommendations", {
        body: null,
        parameters: {title:rawTitle},
        method: "get"
    }).done(function (results) {      
        showRecommendations(results.result);        
    }, function(error) {
        //error handling...
    });

function showRecommendations(items){    
    if(items == null){
        return;
    }  

    if (items.length <= 0){
        return;
    }


    $("#recommendation_panel").show();
    var lst = $("#recommendation_body").append('<div class="list-group"/>'); 
    
    items.forEach(function(r,i,a) {
         lst.append('<a class="list-group-item" href="' + r.link + '">'+r.title+'</a></br>');   
    })

    $("#recommendation_header").append("Recommended Posts"); 
    $("#recommendation_footer").append("Powered by Azure Machine Learning"); 
}   
