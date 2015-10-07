$(document).ready(function () {
    var $genereEditor = $(".song-genere-editor");

    $genereEditor
    .find(".genere-select")
    .on("click", "> li > a", function (e) {
        e.preventDefault();

        var $this = $(this);
        var $genereParent = $this.closest("li");

        $genereParent.toggleClass("selected");

        var selected = $genereParent.hasClass("selected");
        $genereParent.find(".selected-input").val(selected);
    });


    var $likeButton = $(".btn-like");

    $likeButton
        .on("click", function (e) {
            e.preventDefault();

            var $this = $(this);
            var id = $this.attr("data-me");

            $.ajax({
                type: "POST",
                url: "/Songs/Like/?id=" + id,
                contentType: "application/json;charset=utf-8",
                processData: true,
                success: function (data, status, xhr) {
                    if (data.code != "error") {
                        var $likesCount = $("#" + id);
                        $likesCount.html(data.likes);
                    }

                },
                error: function (xhr) {
                    //dont display anything on error..
                }
            });

        });
});