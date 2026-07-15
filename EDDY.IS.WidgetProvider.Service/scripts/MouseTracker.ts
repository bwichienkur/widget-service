class MouseTracker {
    prevEvent: any;
    currentEvent: any;
    eventEmitter: any;
    triggerFunction: any;

    constructor(triggerFunction) {
        this.triggerFunction = triggerFunction;
        window.onmousemove = (event) => { this.runMouseTracker(event) };
    }

    public runMouseTracker(event) {
        this.currentEvent = event;
        var movementX = 0;
        var movementY = 0;

        if (this.prevEvent && this.currentEvent) {
            movementX = Math.abs(this.currentEvent.clientX - this.prevEvent.clientX);
            movementY = Math.abs(this.currentEvent.clientY - this.prevEvent.clientY);
            var movement = Math.sqrt(movementX * movementX + movementY * movementY);
            var speed = 10 * movement;
        }
        this.prevEvent = this.currentEvent;

        if (speed >10 && this.currentEvent.clientY > 0 && this.currentEvent.clientY < 60) {
            this.triggerFunction();
            window.onmousemove = null;
        }
    }
}