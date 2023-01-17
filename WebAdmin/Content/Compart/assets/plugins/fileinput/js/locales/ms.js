/*!
 * FileInput <_LANG_> Translations
 *
 * This file must be loaded after 'fileinput.js'. Patterns in braces '{}', or
 * any HTML markup tags in the messages must not be converted or translated.
 *
 * @see http://github.com/kartik-v/bootstrap-fileinput
 *
 * NOTE: this file must be saved in UTF-8 encoding.
 */
(function ($) {
    "use strict";

    $.fn.fileinputLocales['_LANG_'] = {
        fileSingle: 'fail',
        filePlural: 'fail-fail',
        browseLabel: 'Layari &hellip;',
        removeLabel: 'Buang',
        removeTitle: 'Kosongkan fail yang dipilih',
        cancelLabel: 'Batal',
        cancelTitle: 'Batalkan muat naik yang sedang berjalan',
        uploadLabel: 'Muat naik',
        uploadTitle: 'Muat naik fail-fail yang dipilih',
        msgNo: 'Tidak',
        msgNoFilesSelected: 'Tiada fail yang dipilih',
        msgCancelled: 'Batal',
        msgPlaceholder: 'Pilih {files}...',
        msgZoomModalHeading: 'Paparan Terperinci',
        msgFileRequired: 'Anda mesti pilih fail untuk dimuat naik.',
        msgSizeTooSmall: 'Fail "{name}" (<b>{size} KB</b>) terlalu kecial dan mestilah besar daripada <b>{minSize} KB</b>.',
        msgSizeTooLarge: 'Fail "{name}" (<b>{size} KB</b>) melebihi saiz muat naik maksimum <b>{maxSize} KB</b>.',
        msgFilesTooLess: 'Anda mesti pilih sekurang-kurangnya <b>{n}</b> {files} untuk memuat naik.',
        msgFilesTooMany: 'Jumlah fail yang dipilih untuk dimuat naik <b>({n})</b> melebihi had maksimum yang dibenarkan <b>{m}</b>.',
        msgFileNotFound: 'Fail "{name}" tidak dapat ditemui!',
        msgFileSecured: 'Sekatan keselamatan menghalang membaca fail "{name}".',
        msgFileNotReadable: 'Fail "{name}" tidak dapat dibaca.',
        msgFilePreviewAborted: 'Paparan fail dibatalkan untuk "{name}".',
        msgFilePreviewError: 'Ralat berlaku semasa membaca fail "{name}".',
        msgInvalidFileName: 'Terdapat karakter yang tidak sah atau tidak disokong di dalam fail "{name}".',
        msgInvalidFileType: 'Jenis fail tidak sah untuk fail "{name}". Hanya fail "{types}" yang disokong.',
        msgInvalidFileExtension: 'Format tidak sah untuk fail "{name}". Hanya format "{extensions}" yang disokong.',
        msgFileTypes: {
            'image': 'image',
            'html': 'HTML',
            'text': 'text',
            'video': 'video',
            'audio': 'audio',
            'flash': 'flash',
            'pdf': 'PDF',
            'object': 'object'
        },
        msgUploadAborted: 'Muat naik fail dibatalkan',
        msgUploadThreshold: 'Sedang diproses...',
        msgUploadBegin: 'Memulakan...',
        msgUploadEnd: 'Selesai',
        msgUploadEmpty: 'Tiada data sah tersedia untuk dimuat naik.',
        msgUploadError: 'Ralat',
        msgValidationError: 'Ralat Pengesahan',
        msgLoading: 'Memuatkan {index} fail {files} &hellip;',
        msgProgress: 'Memuatkan {index} fail {files} - {name} - {percent}% selesai.',
        msgSelected: '{n} {files} dipilih',
        msgFoldersNotAllowed: 'Seret & lepas fail sahaja! Langkau {n} fail yang dilepaskan(s).',
        msgImageWidthSmall: 'Lebar fail imej "{name}" mestilah sekurang-kurangnya {size} px.',
        msgImageHeightSmall: 'Ketinggian fail imej "{name}" mestilah sekurang-kurangnya {size} px.',
        msgImageWidthLarge: 'Lebar fail imej "{name}" tidak boleh melebihi {size} px.',
        msgImageHeightLarge: 'Ketinggian fail imej "{name}" tidak boleh melebihi {size} px.',
        msgImageResizeError: 'Tidak boleh mendapatkan dimensi imej untuk mengubah saiz.',
        msgImageResizeException: 'Ralat semasa mengubah saiz imej.<pre>{errors}</pre>',
        msgAjaxError: 'Sesuatu telah berlaku dengan {operation} operasi. Sila cuba sekali lagi!',
        msgAjaxProgressError: '{operation} gagal',
        ajaxOperations: {
            deleteThumb: 'padam fail',
            uploadThumb: 'muat naik fail',
            uploadBatch: 'muat naik fail berkumpulan',
            uploadExtra: 'muat naik borang data'
        },
        dropZoneTitle: 'Seret & lepas fail disni &hellip;',
        dropZoneClickTitle: '<br>(atau klik untuk pilih {files})',
        fileActionSettings: {
            removeTitle: 'Buang fail',
            uploadTitle: 'Muat naik fail',
            uploadRetryTitle: 'Cuba semula memuat naik',
            downloadTitle: 'Muat turun fail',
            zoomTitle: 'Lihat butiran',
            dragTitle: 'Alih / Susun semula',
            indicatorNewTitle: 'Belum dimuat naik',
            indicatorSuccessTitle: 'Telah dimuat naik',
            indicatorErrorTitle: 'Ralat memuat naik',
            indicatorLoadingTitle: 'Memuat naik ...'
        },
        previewZoomButtonTitles: {
            prev: 'Lihat fail sebelumnya',
            next: 'Lihat fail seterusnya',
            toggleheader: 'Togol pengepala',
            fullscreen: 'Togol skrin penuh',
            borderless: 'Togol mod tanpa sempadan',
            close: 'Tutup paparan butiran'
        }
    };
})(window.jQuery);