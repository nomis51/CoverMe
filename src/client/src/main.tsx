import {StrictMode} from 'react'
import {createRoot} from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import {unstable_HistoryRouter as HistoryRouter} from 'react-router-dom'
import {createBrowserHistory} from "history";

export const history = createBrowserHistory() as any

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <HistoryRouter history={history}>
            <App/>
        </HistoryRouter>
    </StrictMode>,
)
