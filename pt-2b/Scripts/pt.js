function ButtonShowLoading(bID)
{
    $('#' + bID).html('<span class="glyphicon glyphicon-refresh glyphicon-refresh-animate"></span> Загрузка...');
    $('#' + bID).prop('disabled', true);
}

function ButtonSetHtml(bID, html)
{
    $('#' + bID).html(html);
    $('#' + bID).prop('disabled', false);
}

function TestJSInitialize()
{
    if (test.currentQuestion == 0)
    {
        $('#backButton').prop('disabled', true);
    }
}

function nextButtonClick()
{
    /*var answerVal = $("input:radio[name ='answerRadio']:checked").val();
    if (answerVal == undefined) return null;

    alert('finish')*/
}

function backButtonClick() {
}

$(document).ready(function () {
    /*if (test != undefined) {
        $('#backButton').click(backButtonClick);
        $('#nextButton').click(nextButtonClick);
        TestJSInitialize();
    }*/
});

function TestSubmitDisable() {
    $('#backButton').prop('disabled', true);
    $('#nextButton').prop('disabled', true);
}