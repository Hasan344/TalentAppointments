// =====================================================================
// QAB Form Validation Helpers
// 1. Səhifə yükləndikdə xətalı ilk sahəyə avtomatik scroll edir.
// 2. Xətalı field-in atasına .qab-form__field--has-error class qoyur (label qırmızılaşsın).
// 3. Xəta olan section-a .qab-form__section--has-errors class qoyur (başlıq qırmızılaşsın).
// 4. Form submit olunduqda jQuery Validation xətalarına da reaksiya verir.
// =====================================================================

(function () {
    'use strict';

    function markErroredFields() {
        // Mövcud highlight-ları təmizlə
        document.querySelectorAll('.qab-form__field--has-error').forEach(function (el) {
            el.classList.remove('qab-form__field--has-error');
        });
        document.querySelectorAll('.qab-form__section--has-errors').forEach(function (el) {
            el.classList.remove('qab-form__section--has-errors');
        });

        // Xətalı input-ların parent qab-form__field-na class əlavə et
        var errored = document.querySelectorAll('.input-validation-error');
        errored.forEach(function (input) {
            var field = input.closest('.qab-form__field');
            if (field) {
                field.classList.add('qab-form__field--has-error');
            }

            var section = input.closest('.qab-form__section');
            if (section) {
                section.classList.add('qab-form__section--has-errors');
            }
        });
    }

    function scrollToFirstError() {
        // 1) Validation summary varsa ona, yoxsa ilk xətalı field-ə scroll et
        var summary = document.querySelector('.qab-form__validation.validation-summary-errors');
        var firstError = document.querySelector('.input-validation-error');
        var alertBlock = document.querySelector('.qab-alert--danger');

        var target = alertBlock || summary || firstError;
        if (target) {
            // Smooth scroll
            target.scrollIntoView({ behavior: 'smooth', block: 'center' });
            // İlk xətalı field-ə focus
            if (firstError && typeof firstError.focus === 'function') {
                setTimeout(function () { firstError.focus(); }, 350);
            }
        }
    }

    // İlk yükləmədə — server-side qaytardığı xətalar üçün
    document.addEventListener('DOMContentLoaded', function () {
        markErroredFields();

        // Yalnız xəta varsa scroll et
        if (document.querySelector('.input-validation-error') ||
            document.querySelector('.qab-form__validation.validation-summary-errors') ||
            document.querySelector('.qab-alert--danger')) {
            scrollToFirstError();
        }

        // Form göndərildikdə client-side validation xətalarını da təqib et
        var forms = document.querySelectorAll('form.qab-form');
        forms.forEach(function (form) {
            form.addEventListener('submit', function () {
                // Submit-dən az sonra DOM yenilənmiş olur, ona görə setTimeout
                setTimeout(function () {
                    markErroredFields();
                    if (document.querySelector('.input-validation-error')) {
                        scrollToFirstError();
                    }
                }, 100);
            });

            // Field-də dəyişiklik olduqda (user düzəliş edir) o field-in xəta highlight-ı silinsin
            form.addEventListener('input', function (e) {
                if (e.target.matches('input, select, textarea')) {
                    if (!e.target.classList.contains('input-validation-error')) {
                        var field = e.target.closest('.qab-form__field');
                        if (field) field.classList.remove('qab-form__field--has-error');
                    }
                }
            }, true);
        });
    });

    // jQuery Validation hook (əgər mövcuddursa)
    if (typeof jQuery !== 'undefined' && jQuery.validator) {
        jQuery(document).ready(function ($) {
            $('form.qab-form').each(function () {
                var $form = $(this);
                var validator = $form.data('validator');
                if (validator) {
                    var origInvalidHandler = validator.settings.invalidHandler;
                    validator.settings.invalidHandler = function (event, vd) {
                        if (typeof origInvalidHandler === 'function') {
                            origInvalidHandler.call(this, event, vd);
                        }
                        setTimeout(function () {
                            markErroredFields();
                            scrollToFirstError();
                        }, 50);
                    };
                }
            });
        });
    }
})();
