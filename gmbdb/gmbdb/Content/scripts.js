$(document).ready(function () {
    
    if ($("#postComment")) {
        var id = $("#postComment").val();
        calcRating(id);
    }

    $("#postComment").on("click", function () {
        var text = $("#commentText").val();
        var id = $("#postComment").val();
        postComment(text, id);
    }); //works 

    $("#likeButton").on("click", function () {
        likeOrDislike(1, $("#likeButton").val());
    }); //works

    $("#dislikeButton").on("click", function () {
        likeOrDislike(2, $("#dislikeButton").val());
    }); //works

    $("#rateButton").on("click", function () {
        var radioValue = $("#ratingRadio:checked").val();
        var id = $("#postComment").val(); //lazysolution to get refview ID
        rateReview(radioValue, id);
    });

    function likeOrDislike(lod, lodval) {

        $.ajax({
            url: "/Reviews/LikeOrDislike",
            type: "POST",
            traditional: true,
            dataType: "json",
            data: {
                val: lod,
                reviewId: lodval
            },

            success: function (status) {

                if (status == 1) {
                    alert("You liked this review!");
                } else if (status == 2) {
                    alert("You disliked this review!");
                } else if (status == 3) {
                    alert("You have already liked or disliked, sorry, no changsies!");
                }

                window.location.reload();
            }
        });
    }

    function postComment(txt, reviewId) {

        $.ajax({
            url: "/Reviews/PostComment",
            type: "POST",
            traditional: true,
            dataType: "json",
            data: { text: txt, reviewId: reviewId },
            success: function (status) {
                if (status == 1) {
                    alert("Comment posted successfully!");
                    window.location.reload();
                } else if (status == 2) {
                    alert("Comment too short!");
                    document.getElementById("commentText").FOCUS();
                }
            }
        });
    }
    function rateReview(radioVal, reviewId) {
        $.ajax({
            url: "/Reviews/RateReview",
            type: "POST",
            dataType: "json",
            data: { value: radioVal, reviewId: reviewId },
            success: function (data) {
                if (data.status == 1) {
                    //$("#ratingText").text(data.totalrating); fallback
                    alert("Successfully rated!!");
                    window.location.reload();
                }
                else if (data.status == 2) {
                    alert("Something went wrong... Reloading page.");
                    window.location.reload();
                }
            }
        });
    }

    function calcRating(reviewId) {
        $.ajax({
            url: "/Reviews/CalcScore",
            type: "POST",
            dataType: "json",
            data: { id: reviewId },
            success: function (calculatedRating) {
                if (calculatedRating > 0) {
                    $("#ratingText").text(calculatedRating.toFixed(2)); //only 2 decimals
                }
                else {
                    $("#ratingText").text("0");
                }
            }
        });
    }
})