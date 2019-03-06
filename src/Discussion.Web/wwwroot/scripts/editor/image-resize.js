
export class ImageResizingPopoverModule {
    
    constructor(context){
        this._context = context;
    }
    
    shouldInitialize(){
        return true;
    }
    
    initialize() {
        const ctx = this._context;
        const ui = $.summernote.ui;
        
        function resize(size){
            return function () {
                const editor = ctx.modules['editor'];
                const $target = $(editor.restoreTarget());

                const src = $target.attr('src');
                const hashIndex = src.lastIndexOf('#');
                let url = hashIndex === -1 ? src : src.substr(0, hashIndex);
                url = url  + (size ? '#' + size : '');

                $target.attr('src', url);
                ctx.modules['handle'].update();
            };
        }
        
        
        this._context.memo('button.imageSizeSmall', function () {
            return ui.button({
                contents: '<span class="note-fontsize-10">小</span>',
                tooltip: '小型图片',
                click: resize('small')
            }).render();
        });
        
        this._context.memo('button.imageSizeMiddle', function () {
            return ui.button({
                contents: '<span class="note-fontsize-10">中</span>',
                tooltip: '中型图片',
                click: resize('middle')
            }).render();
        });
        
        this._context.memo('button.imageSizeRaw', function () {
            return ui.button({
                contents: '<span class="note-fontsize-10">大</span>',
                tooltip: '大型图片',
                click: resize('')
            }).render();
        });

        this._context.invoke('buttons.build',
            this._context.modules['imagePopover'].$popover.find('.popover-content'), 
            this._context.options.popover.image);
    }
    
    
}