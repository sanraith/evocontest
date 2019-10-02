/**
 * Adds progress-bar functionality to file upload forms.
 */

$(function () {
    var _form = null;
    var _fileInput = null;
    var _submitButton = null;
    var _status = null;
    var _progressBar = null;
    var _progressStatus = null;

    function setProgressItemActive(index, isValid) {
        $(".is-active").removeClass("is-active");
        $(".is-done").removeClass("is-done");
        if (isValid === null) {
            $("#progressItem-" + index).addClass("is-active");
        } else if (isValid) {
            $("#progressItem-" + index).addClass("is-done");
        } else {
            $("#progressItem-" + index).addClass("is-active"); // TODO error state
        }
    }

    function uploadFile() {
        setProgressItemActive(1);
        _progressBar.value = 0;
        setProgressDisplay(true);
        setSubmitEnabled(false);
        var file = _fileInput.files[0];

        var formdata = new FormData();
        formdata.append("file1", file);

        var ajax = new XMLHttpRequest();
        ajax.upload.addEventListener("progress", progressHandler, false);
        ajax.addEventListener("load", completeHandler, false);
        ajax.addEventListener("error", errorHandler, false);
        ajax.addEventListener("abort", abortHandler, false);
        ajax.open("POST", _form.action);
        ajax.send(formdata);
    }

    function progressHandler(event) {
        _progressStatus.innerHTML = "Uploaded " + event.loaded + " bytes of " + event.total;
        var percent = (event.loaded / event.total) * 100;
        if (percent > 99) {
            percent = 99; // Do not show 100% until completed event.
        }
        _progressBar.value = Math.round(percent);
        _status.innerHTML = Math.round(percent) + "% uploaded... please wait";
    }

    function completeHandler(event) {
        setProgressDisplay(false);
        if (event.target.status == 200) {
            setProgressItemActive(2);
            reloadForm();
            //countDown("Sikeres feltöltés.");
        } else {
            var result;
            try {
                result = JSON.parse(event.target.responseText);
            } catch (error) {
                result = {};
            }
            var message = result.error ? result.error : "Ismeretlen hiba történt a feltöltés során.";
            _status.innerHTML = message;
            $("#btn-back").show();
        }
    }

    function errorHandler(event) {
        setSubmitEnabled(true);
        setProgressDisplay(false);
        _status.innerHTML = "Feltöltés sikertelen.";
    }

    function abortHandler(event) {
        setSubmitEnabled(true);
        setProgressDisplay(false);
        _status.innerHTML = "Feltöltés megszakítva.";
    }

    function countDown(message) {
        var countDownDate = new Date();
        countDownDate = countDownDate.setSeconds(countDownDate.getSeconds() + 6);

        var onCountDownInterval = function () {
            var now = new Date().getTime();
            var distance = countDownDate - now;
            var seconds = Math.floor((distance % (1000 * 60)) / 1000);
            _status.innerHTML = message + " Frissítés " + seconds + " másodperc múlva...";
            if (distance < 1000) {
                clearInterval(countDownInterval);
                location.reload();
            }
        };
        var countDownInterval = setInterval(onCountDownInterval, 200);
        onCountDownInterval();
    }

    function setSubmitEnabled(isEnabled) {
        $(_submitButton).prop("disabled", isEnabled ? null : "disabled");
        if (isEnabled) {
            $(_submitButton).show("fast");
        } else {
            $(_submitButton).hide();
        }
    }

    function setProgressDisplay(isDisplayed) {
        if (isDisplayed) {
            $(_progressBar).show();
        } else {
            $(_progressBar).hide();
            _progressStatus.innerHTML = "";
        }
    }

    function initUploadForm() {
        _form = $(".upload_form")[0];
        if (!_form) { return; }

        _fileInput = $(_form).find("input[type='file']")[0];
        _submitButton = $(_form).find("input[type='submit']")[0];
        _progressBar = $(_form).find("progress")[0];
        _status = $(_form).find(".upload_form_status")[0];
        _progressStatus = $(_form).find(".upload_form_progress_status")[0];

        if (!_submitButton || !_submitButton || !_status || !_progressBar || !_progressStatus) { return; }

        setSubmitEnabled(false);
        $(_fileInput).change(function (event) {
            setSubmitEnabled(_fileInput.files.length == 1);
        });
        $(_submitButton).click(function (event) {
            if (event.isDefaultPrevented()) {
                return;
            }
            event.preventDefault();
            uploadFile();
        });
    }

    function reloadForm() {
        $("#submitContent").load("CurrentSubmission", () => initUploadForm());
        //initUploadForm();
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/userhub")
        .build();

    connection.start().then(function () {
        console.log("connected");
    });

    connection.on("UpdateUploadStatus", (state, isValid, error) => {
        setProgressItemActive(state, isValid);
        reloadForm();
    });

    reloadForm();
});
