
class App
{
    private readonly apiPath = '/api/v1/media/'
    private volumeStep = 5
    // private online = true

    private _online = true
    public get online(): boolean
    {
        return this._online
    }
    public set online(v: boolean)
    {
        this._online = v
        let offlineUI = document.querySelector('#offline') as HTMLElement
        offlineUI.hidden = v
    }

    private _error: string
    public get error(): string
    {
        return this._error
    }
    public set error(v: string)
    {
        this._error = v
        let errorUI = document.querySelector('#error') as HTMLElement
        errorUI.innerText = v
        errorUI.hidden = !v
    }

    constructor()
    {
        this.init()
    }

    private init()
    {
        // https://stackoverflow.com/a/36249012/451043
        let controls = Array.from(document.querySelectorAll('.controls button'))
        for (const button of controls)
        {
            button.addEventListener('click', event => this.commandClick(event))
        }

        // add events

        // https://developer.mozilla.org/en-US/docs/Web/API/Page_Visibility_API
        if (typeof document.hidden != undefined)
        {
            document.addEventListener('visibilitychange', () =>
            {
                if (!document.hidden)
                {
                    this.checkConnection()
                }
            })
        }
    }

    private commandClick(event: Event)
    {
        let element = event.currentTarget as HTMLButtonElement
        this.sendCommand(element.name)
    }

    private sendCommand(cmd: string)
    {
        if (cmd == 'volume-up' || cmd == 'volume-down')
            cmd += this.volumeStep

        // https://developers.google.com/web/updates/2015/03/introduction-to-fetch
        fetch(this.apiPath + cmd, { method: 'POST' })
            .then(response =>
            {
                if (this.error)
                    this.error = ''

                if (!this.online)
                    this.checkConnection()
            })
            .catch(error =>
            {
                this.error = `Command '${cmd}' failed.`

                if (this.online)
                    this.checkConnection()
            })
    }

    private checkConnection()
    {
        fetch(this.apiPath + 'test')
            .then(response =>
            {
                this.online = true
            })
            .catch(error =>
            {
                this.online = false
            })
    }
}

let app = new App()
