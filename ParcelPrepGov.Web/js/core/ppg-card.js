$(function ($) {

    var defaults = {};

    function plugin(element, options) {
        this.$element = $(element);
        // merge the user's configuration into the defaults
        this.settings = $.extend({}, defaults, options);
        this.init();
    };

    plugin.prototype = {
        init: function () {
            const $card = this.$element; // ppg-card
            const $header = $card.find('.ppg-card-header-cols');
            const $bodyWrapper = $card.find('.ppg-card-body-wrapper');
            const $body = $card.find('.ppg-card-body');

            // insert chevron
            $header.append('<div style="grid-column: 11 / span 2;justify-self: end;"><i></i></div>');
            $header.css('cursor', 'pointer');

            // order is important
            // wait to select because it wasn't appended until now
            const $icon = $header.find('i');

            $icon.dxButton({ icon: 'chevrondown' });

            $header.on('click', function (e) {

                $bodyWrapper.slideToggle('slow', function () {
                    if ($icon.has('.dx-icon-chevrondown').length) {
                        $icon.dxButton({ icon: 'chevronup' });
                    } else {
                        $icon.dxButton({ icon: 'chevrondown' });
                    }
                });

            });

        }
    };

    $.fn.ppgcard = function (options) {
        // http://api.jquery.com/data/
        // Store arbitrary data associated with the matched elements or return the value at the named data store for the first element in the set of matched elements.
        // if plugin initialized return else initialize and cache in data
        return this.each(function () {
            if (!$.data(this, 'ppgcard')) {
                $.data(this, 'ppgcard', new plugin(this, options));
            }
        });
    };

}(jQuery));