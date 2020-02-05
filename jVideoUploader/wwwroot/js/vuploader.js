// plupload selected files
var _selectedFiles = [];
// plupload uploaded files
var _uploadedFiles = [];

var publishingSettings = initMainTemplate();
var selectedFileIndex = 0;
var totalTemplates = 0;
var templateIndex = 0;
var selectedObject;

processTemplates();
console.log(templates);

$(function () {
    /***********************************************************
     * // START UPLOADER SCRIPT
     * ********************************************************/
    var uploader = new plupload.Uploader({
        runtimes: 'html5,flash,silverlight,html4',
        browse_button: 'pickfiles', // you can pass an id...
        container: 'plupload_container',
        drop_element: 'plupload_container', // 'FileUploadContainer',
        multi_selection: true,
        unique_names: true,
        chunk_size: '8mb',
        url: plupload_settings.handlerpath,
        flash_swf_url: '/js/plupload/js/Moxie.swf',
        silverlight_xap_url: '/js/plupload/js/Moxie.xap',
        headers: { UGID: '0', UName: plupload_settings.username },
        filters: {
            max_file_size: plupload_settings.maxfilesize,
            mime_types: [
                { title: plupload_settings.extensiontitle, extensions: plupload_settings.extensions }
            ]
        },
        init: {
            PostInit: function () {
                _selectedFiles = [];
                _uploadedFiles = [];
                /*document.getElementById('filelist').innerHTML = '';

                document.getElementById('uploadfiles').onclick = function () {
                    uploader.start();
                    return false;
                };*/
            },
            FilesAdded: function (up, files) {
                var _max_files = plupload_settings.maxallowedfiles;
                _selectedFiles = files;
                if (_selectedFiles.length > _max_files) {
                    $('#progress_container').html('You can\'t upload more than ' + plupload_settings.maxallowedfiles + ' files');
                    $.each(files, function (i, file) {
                        uploader.removeFile(file);
                    });
                    _selectedFiles = [];
                    $('#uploadfiles').hide();
                } else {
                    this.message = '';
                    for (let i = 0; i <= _selectedFiles.length - 1; i++) {
                        _selectedFiles.css = 'progress-bar-danger';
                        _selectedFiles.percent = 0;
                        $('#progress_container')
                            .append('<div class="m-b-5">' + _selectedFiles[i].name + '</div><div class="progress"><div id="progress_' + _selectedFiles[i].id + '" class="progress-bar" role="progressbar" style="width: 0%;" aria-valuemin="0" aria-valuemax="100"><span id="pvalue_' + _selectedFiles[i].id + '">0%</span></div></div>');
                    }
                    $('#pickfiles').hide();

                    uploader.start();

                }

                up.refresh();
            },
            UploadProgress: function (up, file) {
                $('#progress_' + file.id).attr('style', 'width: ' + file.percent + '%');
                $('#pvalue_' + file.id).html(file.percent + '%');

                for (let i = 0; i <= _selectedFiles.length - 1; i++) {
                    if (file.id === _selectedFiles[i].id) {
                        _selectedFiles[i].percent = file.percent;
                    }
                }
            },
            Error: function (up, err) {
                if (err.code.toString() === '-601') {
                    $('#modalmsg').append(displayMessage("Invalid extension! please select proper files", 'alert-danger'))
                } else if (err.code.toString() === '-600') {
                    $('#modalmsg').append(displayMessage("File too large, Maximum size " + plupload_settings.maxfilesize + " allowed", 'alert-danger'))
                } else {
                    $('#modalmsg').append('<div>Error: ' + err.code +
                        ', Message: ' + err.message +
                        (err.file ? ', File: ' + err.file.name : '') +
                        '</div>'
                    );
                }

                up.refresh(); // Reposition Flash/Silverlight
            },
            FileUploaded: function (up, file, info) {
                const rpcResponse = JSON.parse(info.response);
                // let result = '';
                this.showProgress = false;
                if (typeof (rpcResponse) !== 'undefined' && rpcResponse.result === 'OK') {
                    _uploadedFiles.push(rpcResponse);

                    $('#progress_' + file.id).addClass('bg-success');
                    for (let i = 0; i <= _selectedFiles.length - 1; i++) {
                        if (file.id === _selectedFiles[i].id) {
                            _selectedFiles[i].percent = 100;
                            _selectedFiles[i].css = 'progress-bar-success';
                        }
                    }
                    if (_selectedFiles.length === _uploadedFiles.length) {

                        // all files uploaded
                        $('#plupload_container').hide();
                        // prepare data
                        _uploadedFiles.forEach(function (item) {
                            publishingSettings.inputs.push({
                                key: item.fname,
                                isCompleted: false,
                                isStarted: false,
                                hasErrors: false,
                                templates: initPubTemplate()
                            });
                        });
                        $('#publish-panel').show();
                        // reset plupload files
                        _selectedFiles = [];
                        _uploadedFiles = [];

                        // start publishing
                        publish();

                    }

                } else {
                    let code;
                    let message;
                    if (typeof (rpcResponse.error) !== 'undefined') {
                        code = rpcResponse.error.code;
                        message = rpcResponse.error.message;
                        if (message === undefined || message === '') {
                            message = rpcResponse.error.data;
                        }
                    } else {
                        code = 0;
                        message = 'Error uploading the file to the server';
                    }
                    uploader.trigger('Error', {
                        code: code,
                        message: message,
                        file: ''
                    });
                }
            }
        }
    });
    uploader.init();

    /***********************************************************
     *  // END UPLOADER SCRIPT
     *  *******************************************************/
    $(".publish").on({
        click: function (e) {
            var id = $(this).data('id');
            $('#extended_' + id).show();
            $('#normal_' + id).hide();
            return false;
        }
    }, '.fileitem');
    $(".publish").on({
        click: function (e) {
            var id = $(this).data('id');
            $('#extended_' + id).hide();
            $('#normal_' + id).show();
            return false;
        }
    }, '.closebtn');

});



function increment_publishing() {
    if (templateIndex >= (totalTemplates - 1)) {
        templateIndex = 0;
        // increment file
        if (selectedFileIndex >= (publishingSettings.inputs.length - 1)) {
            $('#pub-Container').hide();
            $('#display-info').show();
            $('#video-output').text(JSON.stringify(publishingSettings, null, 4));
        } else {
            $('#extended_' + publishingSettings.inputs[selectedFileIndex].id).hide();
            $('#normal_' + publishingSettings.inputs[selectedFileIndex].id).show();
            $('#normal_' + publishingSettings.inputs[selectedFileIndex].id).removeClass("card-warning");
            $('#normal_' + publishingSettings.inputs[selectedFileIndex].id).addClass("card-success");
            selectedFileIndex++;
        }
    } else {
        templateIndex++;
    }
    incrementFileProcess();
}

function publish() {
    // Make sure atleast one source file provided
    if (publishingSettings.inputs.length === 0) {
        $('#msg').html(displayMessage('No input provided', 'alert-danger'));
        return;
    }
    console.log(publishingSettings);
    // prepare processing template
    var id = 1;
    publishingSettings.inputs.forEach(function (item) {
        item.id = id;
        if (!item.isStarted) {
            // publishing not yet started on selected file
            $('#pub-Container').append(prepareCard(item, id, 'card-gray'));
            $('#pub-Container').append(extendedContainer(item, id, true));
        } else if (item.isCompleted) {
            if (item.hasErrors)
                $('#pub-Container').append(prepareCard(item, id, 'card-error'));
            else
                $('#pub-Container').append(prepareCard(item, id, 'card-success'));
            $('#pub-Container').append(extendedContainer(item, id, true));
        } else {
            $('#pub-Container').append(extendedContainer(item, id, false));
        }
        id++;
    });
    // start simulation
    incrementFileProcess();
}

function incrementFileProcess() {
    var index = 0;
    publishingSettings.inputs.forEach(function (item) {
        totalTemplates = item.templates.length;
        if (selectedFileIndex === index) {
            item.isStarted = true;
            $('#extended_' + item.id).show();
            $('#normal_' + item.id).hide();
            $('#normal_' + item.id).removeClass("card-gray");
            $('#normal_' + item.id).addClass("card-warning");
            processItemTemplate(item);
        }
        index++;
    });
}

function processItemTemplate(item) {
    var index = 0;
    item.templates.forEach(function (template) {
        if (templateIndex === index) {
            template.status = "Progress";
            template.progress = 0;
            $('#pbar-' + item.id + '-' + template.id).css({ 'width': template.progress + '%' });
            $('#pbar-' + item.id + '-' + template.id).removeClass('bg-dark');
            $('#pbar-' + item.id + '-' + template.id).addClass('bg-primary');
            $('#pbar-' + item.id + '-' + template.id).html(template.progress + '%');

            //prepare submit data
            var object = {
                tp: 0,
                key: item.key,
                pid: 0,
                template: {
                    presetID: template.presetID,
                    prefix: template.prefix,
                    errorText: template.errorText,
                    applyMeta: template.applyMeta,
                    generateThumbnails: template.generateThumbnails
                }
            }
            initPub(object);
        }
        index++;
    });
}

function initPub(object) {
    selectedObject = object;
    $.ajax({
        type: 'POST',
        url: plupload_settings.encodingpath,
        data: JSON.stringify([object]),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            if (parseInt(data.ecode, 10) > 0) {
                // error occured
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].errorcode = parseInt(data.ecode, 10);
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].edesc = data.edesc;
            } else {
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].processID = data.procid;
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].IntervalID = setInterval(function () {
                    getProgress();
                }, 2000);
            }
        }
    });
}

function getProgress() {

    selectedObject.pid = publishingSettings.inputs[selectedFileIndex].templates[templateIndex].processID;
    selectedObject.tp = 1;
    $.ajax({
        type: 'POST',
        url: plupload_settings.encodingpath,
        data: JSON.stringify([selectedObject]),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            publishingSettings.inputs[selectedFileIndex].templates[templateIndex].progress = parseInt(data.status, 10);
            $('#pbar-' + publishingSettings.inputs[selectedFileIndex].id + '-' + publishingSettings.inputs[selectedFileIndex].templates[templateIndex].id).css({ 'width': data.status + '%' });
            // $('#pbar-' + publishingSettings.inputs[selectedFileIndex].id + '-' + publishingSettings.inputs[selectedFileIndex].templates[templateIndex].id).removeClass('progress-bar-danger');
            $('#pbar-' + publishingSettings.inputs[selectedFileIndex].id + '-' + publishingSettings.inputs[selectedFileIndex].templates[templateIndex].id).html(data.status + '%');
            // $('#progress-status').html(Progress: ' + selectedFile.progress + '%');
            if (parseInt(data.status, 10) >= 100) {
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].status = 'completed';
                // $('#progress-status').html('File: ' + selectedFile.sf + ', Status: Processing Data');
                $('#pbar-' + publishingSettings.inputs[selectedFileIndex].id + '-' + publishingSettings.inputs[selectedFileIndex].templates[templateIndex].id).css({ 'width': '100%' });
                $('#pbar-' + publishingSettings.inputs[selectedFileIndex].id + '-' + publishingSettings.inputs[selectedFileIndex].templates[templateIndex].id).removeClass('bg-primary').addClass('bg-success');
                if (publishingSettings.inputs[selectedFileIndex].templates[templateIndex].IntervalID !== 0) {
                    clearInterval(publishingSettings.inputs[selectedFileIndex].templates[templateIndex].IntervalID);
                }
                getInfo();
            }
        }
    });
}

function getInfo() {
    selectedObject.pid = publishingSettings.inputs[selectedFileIndex].templates[templateIndex].processID;
    selectedObject.tp = 2;
    $.ajax({
        type: 'POST',
        url: plupload_settings.encodingpath,
        data: JSON.stringify([selectedObject]),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            if (data.status === 'OK') {
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].errorcode = data.ecode;
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].publishedFileName = data.fname;
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].duration = data.dur;
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].dursec = data.dursec;
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].tfile = data.tfile;
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].edesc = data.edesc;
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].isenable = data.isenable;
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].fIndex = data.fIndex;
                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].img_url = data.img_url;
                if (data.fIndex !== undefined) {
                    for (let i = 1; i <= 14; i++) {
                        let _name = '';
                        if (i <= 9) {
                            _name = data.fIndex + '00' + i + '.jpg';
                        } else {
                            _name = data.fIndex + '0' + i + '.jpg';
                        }
                        let _selected = false;
                        if (i === 8) {
                            publishingSettings.inputs[selectedFileIndex].templates[templateIndex].defaultImg = _name;
                            _selected = true;
                        } else {
                            _selected = false;
                        }
                        publishingSettings.inputs[selectedFileIndex].templates[templateIndex].thumbs.push({
                            id: i,
                            filename: _name,
                            selected: _selected
                        });

                        if (publishingSettings.inputs[selectedFileIndex].templates[templateIndex].generateThumbnails) {
                            $('#pthumbs-' + publishingSettings.inputs[selectedFileIndex].id + '-' + publishingSettings.inputs[selectedFileIndex].templates[templateIndex].id).append("<div class='col-md-3'><img class='img-fluid' src='/uploads/thumbnails/" + _name + "'</div>");
                        }
                    }
                }

            } else {

                publishingSettings.inputs[selectedFileIndex].templates[templateIndex].errorText = "Publishing Failed";
            }

            // next video
            increment_publishing();
        }
    });
}

function prepareCard(obj, id, css) {
    var sTr = basicContainer('<h5>' + obj.key + '</h5>', id, css);
    return sTr;
}

function extendedContainer(obj, id, isHidden) {
    var _hidden = '';
    if (isHidden)
        _hidden = "style='display:none;'";
    var sTr = '<div data-id="' + id + '" id="extended_' + id + '" class="card m-b-20 fileitem" ' + _hidden + '>';
    sTr += '<div class="card-body">';

    if (obj.isCompleted || !obj.isStarted) {
        sTr += '<button data-id="' + id + '" type="button" class="btn btn-default btn-xs float-right closebtn">';
        sTr += '<span aria-hidden="true">&times;</span>';
        sTr += '</button>';
    }

    sTr += '<h5 class="card-title m-b-20">' + obj.key + '</h5>';

    obj.templates.forEach(function (template) {
        sTr += prepareProgress(obj, template);
    });
    sTr += '</div></div>';
    return sTr;
}

function prepareProgress(item, obj) {
    var sTr = '<div class="row">';
    sTr += '<div class="col-md-1">' + obj.key + '</div>';
    sTr += '<div class="col-md-11">';
    var css = 'bg-dark';
    if (obj.errorText !== '') {
        sTr += obj.errorText;
        css = 'bg-danger';
    } else if (obj.progress > 0 && obj.progress <= 100) {
        css = 'bg-primary';
    } else if (obj.progress === 100) {
        css = 'bg-success';
    }

    sTr += '<div class="progress m-t-5 m-b-10">';
    sTr += '<div id="pbar-' + item.id + '-' + obj.id + '" class="progress-bar ' + css + '" style="width:' + obj.progress + '%;">' + obj.progress + '%</div>';
    sTr += '</div>';
    // add thumbnail holder
    if (obj.generateThumbnails) {
        sTr += '<div class="row" id="pthumbs-' + item.id + '-' + obj.id + '"></div>';
    }
    sTr += '</div>';
    sTr += '</div>';
    return sTr;
}
function basicContainer(html, id, css) {
    var sTr = '<div data-id="' + id + '" id="normal_' + id + '" class="card ' + css + ' m-b-20 fileitem">';
    sTr += '<div class="card-body">';
    sTr += html;
    sTr += '</div></div>';
    return sTr;
}


function displayMessage(message, css) {
    var sTr = '<div class="alert ' + css + '" role="alert">';
    sTr += message;
    sTr += '<button type="button" class="close" data-dismiss="alert" aria-label="Close">';
    sTr += '<span aria-hidden="true">&times;</span>';
    sTr += '</button>';
    sTr += '</div>';
    return sTr;
}

function initMainTemplate() {
    return {
        inputs: []
    };
}

function processTemplates() {
    var index = 0;
    templates.forEach(function (template) {
        template.id = index + 1;
        template.status = 'pending';
        template.progress = 0;
        template.errorText = '';
        template.processID = 0;
        template.thumbs = [];
        index++;
    });
}

function initPubTemplate() {
    return templates;
}