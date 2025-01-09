
function onFeatureCheckboxChange(checkbox) {
    var featureId = checkbox.id.split('-')[1]; // Get the feature ID from 'feature-{FeatureValue}'
    var isChecked = checkbox.checked;


    if (isChecked) {
        // Show sub-features
        $("#subFeatures-" + featureId).show();
    } else {
        $("#subFeatures-" + featureId).hide();
    }
}

window.OnRoleChange = function (dropdown) {
    var role = dropdown.value;
    alert(role);
    $.ajax({
        url: '@Url.Action("GetFeatureByRole", "Admin")',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ ID: role }),
        success: function (response) {
            const filteredData = response.map(item => ({
                text: item.text,
                value: item.value
            }));

            $('.process-checkbox').prop('checked', false);
            $('.sub-feature-checkbox').prop('checked', false);

            filteredData.forEach(function (item) {

                $('#feature-' + item.value).prop('checked', true);

                
                $('input[name="subFeature-' + item.value + '"]').each(function () {
                    console.log("Checking:", $(this).val(), "with", item.value);
                    if ($(this).val() === item.subFeatureValue) {
                        $(this).prop('checked', true); // Mark sub-feature as checked
                    }
                });
            });

            // After setting checkboxes, show/hide the sub-feature sections accordingly
            filteredData.forEach(function (item) {
                if ($('#feature-' + item.value).prop('checked')) {
                    $("#subFeatures-" + item.value).show(); // Show the sub-features for this feature
                }
            });
        },
        error: function (xhr, status, error) {
            console.error("Error fetching sub-features for the role:", error);
            alert('Failed to fetch sub-features. Please try again.');
        }
    });
};