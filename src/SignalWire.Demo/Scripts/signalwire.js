

(function($) {

    $.wire = {
        get: function(collection, hub) {
            var data = [];
            var c = collection;
            var h = (hub == null || hub == "") ? "data" : hub;

            function wrapDeferred(action) {
                var d = $.Deferred();
                action.done(function(result) {
                    if (result.Success) {
                        d.resolve(result.Data);
                    }
                    else {
                        d.reject(result);
                    }
                }
                ).fail(function (e) {
                    d.reject(e);
                });

                return d;
            }

            return {
                add: function(obj) {
                    return wrapDeferred($.connection[h].add(c, obj));
                },
                update: function(obj) {
                    return wrapDeferred($.connection[h].update(c, obj));
                },
                remove: function(obj) {
                    return wrapDeferred($.connection[h].remove(c, obj));
                },
                read: function(qry) {
                    return wrapDeferred($.connection[h].read(c, qry==null?qry : {}));
                },
                query: function (qry) {
                    return wrapDeferred($.connection[h].query(qry));
                }                
            };
        },

        init: function(hub) {
            var d = $.Deferred();
            var data = [];
            var that = this;
            var h = (hub == null || hub == "") ? "data" : hub;
            $.connection.hub.start().done(function() {
                $.connection[h].getCollections().done(function(result) {
                    $.each(result, function(key, val) {
                        $.wire[val] = that.get(val, h);
                    });
                    d.resolve();
                }).fail(function() { d.reject(); });
            });
            return d;
        }        
    };
})(jQuery);