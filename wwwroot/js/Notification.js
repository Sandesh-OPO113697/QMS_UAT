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
           
            if (!data || !data.fileBase64 || !data.attachmentFileName) {
                $('#mailModal').modal('hide');
                return;
            }
            $('#mailModalSubject').text(data.subject);
            $('#mailModalBody').html(data.body);
            const fileExtension = data.attachmentFileName.split('.').pop().toLowerCase(); // Extract extension

     
            console.log('File Extension:', fileExtension);

            if (fileExtension === 'pdf') {
                console.log("PDF file detected.");
                // Handle PDF case
                handlePDF(data);
            } else if (["jpg", "jpeg", "png", "gif", "bmp", "webp"].includes(fileExtension)) {
                console.log("Image file detected.");
                // Handle image case
                handleImage(data);
            } else if (["xlsx", "xls", "csv"].includes(fileExtension)) {
                console.log("Spreadsheet file detected.");
                // Handle spreadsheet case
                handleSpreadsheet(data);
            } else {
                console.log("Unknown file type.");
                // Handle unknown file type
                handleUnknownFile();
            }

         
          
        },
        error: function (xhr) {
            console.error("Error loading notification:", xhr);
            $('#mailModalAttachment').html(`<p style="color:red;">Error loading notification.</p>`);
            $('#mailModal').modal('hide');
        }
    });
});

function showRestrictedModal() {
    $('#mailModal').modal({
        backdrop: 'static',
        keyboard: false
    });
}
function handlePDF(data) {
    const cleanedBase64 = data.fileBase64.replace(/\s/g, '');
    const byteCharacters = atob(cleanedBase64);
    const byteArray = new Uint8Array(byteCharacters.length);

    for (let i = 0; i < byteCharacters.length; i++) {
        byteArray[i] = byteCharacters.charCodeAt(i);
    }

    const file = new Blob([byteArray], { type: 'application/pdf' });
    const url = URL.createObjectURL(file); // Create a URL for the Blob

    const html = `
        <iframe src="${url}" width="100%" height="500px" style="border: 1px solid #ccc; border-radius: 6px;"></iframe>
       
    `;
    $('#mailModalAttachment').html(html);
   
    showRestrictedModal();
}

// Example function to handle Image
function handleImage(data) {
    const byteCharacters = atob(data.fileBase64);
    const byteArray = new Uint8Array(byteCharacters.length);

    for (let i = 0; i < byteCharacters.length; i++) {
        byteArray[i] = byteCharacters.charCodeAt(i);
    }

    const file = new Blob([byteArray], { type: `image/${data.attachmentFileName.split('.').pop().toLowerCase()}` });
    const url = URL.createObjectURL(file);

    const html = `<img src="${url}" alt="Attachment Image" style="max-width: 100%; height: auto; border-radius: 6px; box-shadow: 0 0 8px rgba(0,0,0,0.1);">`;
    $('#mailModalAttachment').html(html);

    showRestrictedModal();
}

// Example function to handle Spreadsheet
function handleSpreadsheet(data) {
    const byteCharacters = atob(data.fileBase64);
    const byteArray = new Uint8Array(byteCharacters.length);

    for (let i = 0; i < byteCharacters.length; i++) {
        byteArray[i] = byteCharacters.charCodeAt(i);
    }

    const file = new Blob([byteArray], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    const reader = new FileReader();

    reader.onload = function (e) {
        const result = e.target.result;
        const workbook = XLSX.read(result, { type: 'binary' });
        const sheetName = workbook.SheetNames[0];
        let rawHtml = XLSX.utils.sheet_to_html(workbook.Sheets[sheetName]);

        rawHtml = rawHtml
            .replace(/<table[^>]*>/, `<table style="width: 100%; border-collapse: collapse; font-family: 'Segoe UI', sans-serif; font-size: 14px; margin-top: 10px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05); overflow-x: auto;">`)
            .replace(/<thead>/, `<thead style="background-color: #f1f5f9;">`)
            .replace(/<th[^>]*>/g, `<th style="background-color: #e8f0fe; border: 1px solid #d0d7de; padding: 12px 14px; text-align: left; font-weight: 600; color: #333; white-space: nowrap; border-bottom: 2px solid #d0d7de;">`)
            .replace(/<td[^>]*>/g, `<td style="border: 1px solid #e0e0e0; padding: 10px 12px; color: #444; background-color: #ffffff;">`);

        const html = `
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
     
        showRestrictedModal();
    

    };

    reader.readAsBinaryString(file);
}

// Example function to handle Unknown files
function handleUnknownFile() {
    $('#mailModalAttachment').html(`<p style="color:red;">Cannot preview this file type.</p>`);

    showRestrictedModal();
}
