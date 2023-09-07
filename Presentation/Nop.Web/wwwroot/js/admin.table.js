var entityMap = {
  '&': '&amp;',
  '<': '&lt;',
  '>': '&gt;',
  '"': '&quot;',
  "'": '&#39;',
  '/': '&#x2F;',
  '`': '&#x60;',
  '=': '&#x3D;'
};

function escapeHtml(string) {
  if (string == null) {
    return '';
  }
  return String(string).replace(/[&<>"'`=\/]/g, function (s) {
    return entityMap[s];
  });
}

//selectedIds - This variable will be used on views. It can not be renamed
var selectedIds = [];


function clearMasterCheckbox(tableSelector) {
  $($(tableSelector).parents('.dataTables_scroll').find('input.mastercheckbox')).prop('checked', false).change();
  selectedIds = [];
}


function updateMasterCheckbox(tableSelector) {
  var selector = 'mastercheckbox';
  var numChkBoxes = $('input[type=checkbox][class!=' + selector + '][class=checkboxGroups]', $(tableSelector)).length;
  var numChkBoxesChecked = $('input[type=checkbox][class!=' + selector + '][class= checkboxGroups]:checked', $(tableSelector)).length;

  $('.mastercheckbox', $(tableSelector)).prop('checked', numChkBoxes == numChkBoxesChecked && numChkBoxes > 0);
}

function updateTableSrc(tableSelector, isMasterCheckBoxUsed) {
  var dataSrc = $(tableSelector).DataTable().data();
  $(tableSelector).DataTable().clear().rows.add(dataSrc).draw();
  $(tableSelector).DataTable().columns.adjust();
  
  if (isMasterCheckBoxUsed) {
    clearMasterCheckbox(tableSelector);
  }
}


function updateTable(tableSelector, isMasterCheckBoxUsed) {
  $(tableSelector).DataTable().ajax.reload();
  $(tableSelector).DataTable().columns.adjust();

  if (isMasterCheckBoxUsed) {
    clearMasterCheckbox(tableSelector);
  }
}


function updateTableWidth(tableSelector) {
  if ($.fn.DataTable.isDataTable(tableSelector)) {
    $(tableSelector).DataTable().columns.adjust();
  }
}


function initializeDataTableWithCookie(gridSelector) {
  var cookieName = gridSelector.replace('#', '') + "-pageLength";

  $(document).ready(function () {
    // 쿠키에서 페이지 크기 값을 불러옵니다.
    var defaultPageLength = parseInt(getCookie(cookieName)) || 20;

    // 쿠키에 값이 초기에 없을 경우 기본값을 설정
    if (!getCookie('dataTablePageLength')) {
      document.cookie = "dataTablePageLength=" + defaultPageLength + ";path=/";
    }

    // 해당 DataTables 인스턴스를 찾아서 페이지 크기를 설정합니다.
    var table = $(gridSelector).DataTable();
    table.page.len(defaultPageLength).draw();

    // 페이지 크기가 변경될 때 해당 값을 쿠키에 저장합니다.
    $(gridSelector).on('length.dt', function (e, settings, len) {
      document.cookie = cookieName + "=" + len + ";path=/";
    });
  });
}

// 쿠키 값을 가져오는 함수입니다.
function getCookie(name) {
  var value = "; " + document.cookie;
  var parts = value.split("; " + name + "=");
  if (parts.length == 2) return parts.pop().split(";").shift();
}