﻿
$(document).ready(function () {
    var $plugin = $('<div>').nestable().data('nestable');
    console.log("plugin: %o", $plugin);

    var extensionMethods = {
        reinit: function () {
            //console.log('reinit: %o', this);

            // alias
            var list = this;

            // remove expand/collapse controls
            $.each(this.el.find(this.options.itemNodeName), function (k, el) {
                //console.log('el: %o', $(el));

                list.expandItem($(el));

                // if has <ol> child - remove previously prepended buttons
                if ($(el).children(list.options.listNodeName).length) {
                    $(el).children('button').remove();
                }
            });

            // remove delegated event handlers
            list.el.off('click', 'button');

            var hasTouch = 'ontouchstart' in document;
            if (hasTouch) {
                list.el.off('touchstart');
                list.w.off('touchmove');
                list.w.off('touchend');
                list.w.off('touchcancel');
            }

            list.el.off('mousedown');
            list.w.off('mousemove');
            list.w.off('mouseup');

            // call init again
            list.init();
        }, // reinit
        managePlaceholderElements: function (rootElId)
        {
            let list = this;
            let rootEl = this.dragRootEl;

            if (!rootEl || rootEl.length === 0) {
                rootEl = $(`#${rootElId}`)
            }
            rootEl.find(`${this.options.itemNodeName}.${list.options.itemClass}`).each(function (index, element) {
                let olList = $(element).children('ol');
                let allLi = $(olList).children().not('.dd-item-placeholder');
                let displayedLi = list.getDisplayedElements(allLi);
                if (displayedLi.length === 0) {
                    $(olList).children(`.${list.options.emptyItemPlaceholder}`).show();
                } else {
                    $(olList).children(`.${list.options.emptyItemPlaceholder}`).hide();
                }
            });
        },

        setPointerEventsOnIcons: function (value) {
            $('.custom-dd-handle').css('pointer-events', value);
        },

        isTargetValid: function (sourceTreeId) {
            return (this.hasNewRoot && this.isAddingNewElement(sourceTreeId) && this.isFieldValueTypeMatching() && this.isProperFieldForMatrixFieldSet())
                || (!this.hasNewRoot && !this.isDragInsidePredefinedItems(sourceTreeId) && this.isFieldValueTypeMatching() && this.isProperFieldForMatrixFieldSet());
        },

        isAddingNewElement: function (sourceTreeId) {
            let targetTreeId = this.dragRootEl.attr('id');
            return targetTreeId === this.options.formPreview && sourceTreeId === this.options.predefinedItemsContainer;
        },

        isDragInsidePredefinedItems: function (sourceTreeId) {
            let targetTreeId = this.dragRootEl.attr('id');
            return targetTreeId === this.options.predefinedItemsContainer && sourceTreeId === this.options.predefinedItemsContainer;
        },

        isProperFieldForMatrixFieldSet: function () {
            let canDragAndDropField = true;
            let newSiblings = $(this.placeEl).siblings();
            let firstNewSibling = newSiblings[0];
            let firstLiParent = $(firstNewSibling).parent().closest('li');

            if (firstLiParent.length) {
                let layoutStyle = firstLiParent.data('layoutstyle');
                if (layoutStyle) {
                    let decodedLayoutStyle = decodeURIComponent(layoutStyle);
                    let layoutStyleObj = JSON.parse(decodedLayoutStyle);

                    if (layoutStyleObj.layoutType === 'Matrix') {
                        let newFieldId = this.dragElementId;
                        let fieldType = $(`li[data-itemtype="field"][data-id="${newFieldId}"]`).first().data('type');

                        if (fieldType != 'radio' && fieldType != 'checkbox')
                           return false;

                        let maxItems = parseInt(layoutStyleObj.maxItems, 10);

                        let olList = firstLiParent.children('ol');
                        let childCount = olList.children().not('.dd-item-placeholder').length;

                        if (childCount > maxItems) {
                            canDragAndDropField = false;
                        }
                    }
                }
            }

            return canDragAndDropField;
        },

        isFieldValueTypeMatching: function () {
            let newSiblings = $(this.placeEl).siblings();
            let firstNewSibling = newSiblings[0];
            let fieldValueType = $(this.dragEl).children(this.options.itemNodeName).first().data('valuetype');
            if ($(firstNewSibling).data('ismatrix') == 'True') {
                return false;
            }

            if (!$(firstNewSibling).data('valuetype') && !fieldValueType) {
                return true;
            }
            return $(firstNewSibling).data('valuetype') === fieldValueType;
        },

        preventDrop: function () {
            $(`.dd-list`)
                .find(`li.dragging[data-id='${this.dragElementId}']`)
                .show()
                .removeClass('dragging');
            $(this.placeEl).remove();
        },

        handleAddNewElement: function () {
            let targetTreeId = this.dragRootEl.attr('id');
            let draggingItem = this.dragEl.children(this.options.itemNodeName)[0];
            this.setStyleIfFieldValueItem(draggingItem);
            this.configureDraggingItemBeforeDrop(draggingItem);
            draggingItem.remove();
            $(draggingItem).find('.predefined-item-placeholder').remove();
            this.placeEl.replaceWith(draggingItem);
            updateNestableData(targetTreeId);
            getNestableFormElements();
        },

        configureDraggingItemBeforeDrop: function (draggingItem) {
            $(draggingItem).children('ol').show();
            $(draggingItem).find(".form-element-item").remove();
            $(draggingItem).find(".dragable-item").show();
            $(draggingItem).find('.d-none').removeClass('d-none');
        },
        setStyleIfFieldValueItem: function (draggingItem) {
            if ($(draggingItem).attr('data-type') === "checkbox" || $(draggingItem).attr('data-type') === "radio") {
                $(draggingItem).addClass("d-flex");
                $(draggingItem).children('.dd-handle')
                    .removeClass("add-new-element");
            }
        },
        handleDrop: function () {
            var draggingListItem = this.dragEl.children(this.options.itemNodeName)[0];
            this.removeDraggedElementFromSource();
            draggingListItem.remove();
            this.placeEl.replaceWith(draggingListItem);
            this.managePlaceholderElements();
            updateNestableData(this.dragRootEl.attr('id'));
        },

        removeDraggedElementFromSource: function () {
            $(`.dd`)
                .find(`li.dragging[data-id='${this.dragElementId}']`)
                .remove();
        },

        getDisplayedElements: function(elements) {
            let result = [];
            $(elements).each(function (index, element) {
                if ($(element).css('display') !== 'none') {
                    result.push(element);
                }
            });
            return result;
        }
    };
    $.extend(true, $plugin.__proto__, extensionMethods);

    $.fn.expandItem = function (nestableId) {
        let nestable = $(`#${nestableId}`).data('nestable');
        if (nestable) {
            nestable.expandItem($(this));
        }
    }

    $.fn.collapseItem = function (nestableId) {
        let nestable = $(`#${nestableId}`).data('nestable');
        if (nestable) {
            nestable.collapseItem($(this));
        }
    }
});
