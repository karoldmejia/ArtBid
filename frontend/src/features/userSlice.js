import { createSlice } from "@reduxjs/toolkit";

const userSlice = createSlice({
    name: "user",
    initialState: {
        token: null,
        role: null,
        id: null,
    },
    reducers: {
        setUserData: (state, action) => {
            state.token = action.payload.token;
            state.role = action.payload.role;
            state.id = action.payload.id;
        },
        logout: (state) => {
            state.token = null;
            state.role = null;
            state.id = null;
        }
    }
});

export const { setUserData, logout } = userSlice.actions;
export default userSlice.reducer;
