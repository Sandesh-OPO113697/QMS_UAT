function handleModalClose() {
    $.ajax({
        url: '/LogIn/NotificationClosed',
        type: 'GET',
        success: function (response) {
            console.log('C# GET method called successfully.');
        }
    });

    $('#mailModal').modal('hide');
}

$(function () {


    $.ajax({
        url: '/LogIn/GetNotification',
        type: 'GET',
        success: function (data) {
            console.log(JSON.stringify(data));
            if (!data || !data.fileBase64 || !data.attachmentFileName) {
                // $('#mailModalAttachment').html('<p style="color:red;">No attachment found.</p>');
                // $('#mailModal').modal('show');
                $('#mailModal').modal('hide');
                return;
            }
            else {
                $('body > *:not(.modal)').css('pointer-events', 'none')
                $('#mailModal').modal({
                    backdrop: 'static',
                    keyboard: false
                });
                $('#mailModal').on('hidden.bs.modal', function () {
                    $('body > *:not(.modal)').css('pointer-events', 'auto');
                });

                $('#mailModalSubject').text(data.subject);

                $('#mailModalBody').html(data.body);
                const fileExtension = data.attachmentFileName.split('.').pop().toLowerCase();
                const byteCharacters = atob(data.fileBase64);
                const byteNumbers = new Array(byteCharacters.length);
                for (let i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }

                const byteArray = new Uint8Array(byteNumbers);
                const file = new Blob([byteArray]);

                let html = "";

                if (["xlsx", "xls", "csv"].includes(fileExtension)) {
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        const data = e.target.result;
                        const workbook = XLSX.read(data, { type: 'binary' });
                        const sheetName = workbook.SheetNames[0];
                        let rawHtml = XLSX.utils.sheet_to_html(workbook.Sheets[sheetName]);

                        rawHtml = rawHtml
                            .replace(/<table[^>]*>/, `
                            <table style="width: 100%; border-collapse: collapse; font-family: 'Segoe UI', sans-serif; font-size: 14px; margin-top: 10px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05); overflow-x: auto;">`)
                            .replace(/<thead>/, `<thead style="background-color: #f1f5f9;">`)
                            .replace(/<th[^>]*>/g, `
                            <th style="background-color: #e8f0fe; border: 1px solid #d0d7de; padding: 12px 14px; text-align: left; font-weight: 600; color: #333; white-space: nowrap; border-bottom: 2px solid #d0d7de;">`)
                            .replace(/<td[^>]*>/g, `
                            <td style="border: 1px solid #e0e0e0; padding: 10px 12px; color: #444; background-color: #ffffff;">`)
                            .replace(/<\/tr>/g, `</tr>`);

                        html = `
                        <div style="max-height: 500px; overflow-x: auto; overflow-y: auto; border: 1px solid #ddd; border-radius: 8px; padding: 10px; background-color: #fefefe; box-shadow: inset 0 0 5px rgba(0, 0, 0, 0.05);">
                            <style>
                                #mailModalAttachment table tr:nth-child(even) td {
                                    background-color: #f9f9f9;
                                }
                                #mailModalAttachment table tr:hover td {
                                    background-color: #eef6ff;
                                    transition: background-color 0.3s;
                                }
                            </style>
                            ${rawHtml}
                        </div>`;

                        $('#mailModalAttachment').html(html);
                        $('#mailModal').modal('show');
                    };

                    reader.readAsBinaryString(file);
                } else if (fileExtension === "pdf") {
                    const url = URL.createObjectURL(file);
                    html = `<iframe src="${url}" width="100%" height="500px" style="border: 1px solid #ccc; border-radius: 6px;"></iframe>`;
                    $('#mailModalAttachment').html(html);
                    $('#mailModal').modal('show');
                } else {
                    html = `<p style="color: red;">Cannot preview this file type: ${fileExtension}</p>`;
                    $('#mailModalAttachment').html(html);
                    $('#mailModal').modal('show');
                }

            }

        },
        error: function (xhr) {
            console.error("Error loading notification:", xhr);
            $('#mailModalAttachment').html(`<p style="color:red;">Error loading notification.</p>`);
            $('#mailModal').modal('hide');
        }
    });

});