// wwwroot/js/site.js
$(document).ready(function () {
    // Initialize DataTable
    var table = $('#dataTable').DataTable({
        responsive: true,
        language: {
            search: "Search:",
            lengthMenu: "Show _MENU_ entries",
            info: "Showing _START_ to _END_ of _TOTAL_ entries",
            paginate: {
                first: "First",
                last: "Last",
                next: "Next",
                previous: "Previous"
            }
        },
        order: [[0, "desc"]],
        columnDefs: [
            {
                targets: -1,
                orderable: false
            }
        ]
    });

    // Auto-hide alerts after 5 seconds
    setTimeout(function () {
        $('.alert').fadeOut('slow');
    }, 5000);

    // Form validation
    $('form').on('submit', function (e) {
        if (!validateDates()) {
            e.preventDefault();
            return false;
        }
    });

    // Date validation function
    function validateDates() {
        var startDate = new Date($('#StartDate').val());
        var endDate = new Date($('#EndDate').val());

        if (startDate > endDate) {
            alert('End date must be after start date');
            return false;
        }
        return true;
    }

    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Confirm delete
    $('.delete-btn').on('click', function (e) {
        if (!confirm('Are you sure you want to delete this entry?')) {
            e.preventDefault();
        }
    });
});