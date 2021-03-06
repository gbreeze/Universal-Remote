
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

    private _loading: boolean
    public get loading(): boolean
    {
        return this._loading
    }
    public set loading(v: boolean)
    {
        this._loading = v
        let controls = document.querySelector('.controls') as HTMLElement
        v ? controls.classList.add('loading') : controls.classList.remove('loading')
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

    private async commandClick(event: Event)
    {
        this.loading = true
        let element = event.currentTarget as HTMLButtonElement
        await this.sendCommand(element.name)
        this.loading = false
    }

    private async sendCommand(cmd: string)
    {
        if (cmd == 'volume-up' || cmd == 'volume-down')
            cmd += this.volumeStep

        // https://developers.google.com/web/updates/2015/03/introduction-to-fetch
        return fetch(this.apiPath + cmd, { method: 'POST' })
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
