// Keyboard Shortcuts Handler
window.GameKeyboard = (function() {
    let blazorComponent = null;
    let enabled = true;
    
    const shortcuts = {
        'Space': 'endTurn',
        'KeyG': 'navigateGalaxy',
        'KeyR': 'navigateResearch',
        'KeyD': 'navigateDiplomacy',
        'KeyF': 'navigateFleets',
        'KeyC': 'navigateColonies',
        'Escape': 'closeModal',
        'F1': 'showHelp',
        'KeyS': 'quickSave',
        'KeyL': 'quickLoad',
        'Digit1': 'selectFleet1',
        'Digit2': 'selectFleet2',
        'Digit3': 'selectFleet3',
    };
    
    function init(componentRef) {
        blazorComponent = componentRef;
        
        document.addEventListener('keydown', handleKeyDown);
    }
    
    function handleKeyDown(e) {
        if (!enabled) return;
        
        // Don't trigger shortcuts when typing in inputs
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA' || e.target.tagName === 'SELECT') {
            return;
        }
        
        const action = shortcuts[e.code];
        if (action && blazorComponent) {
            e.preventDefault();
            blazorComponent.invokeMethodAsync('HandleShortcut', action);
        }
    }
    
    return {
        init: init,
        setEnabled: function(value) {
            enabled = value;
        },
        addShortcut: function(key, action) {
            shortcuts[key] = action;
        }
    };
})();

console.log('⌨️ Keyboard shortcuts loaded');
