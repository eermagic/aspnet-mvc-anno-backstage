const VuePagination = {
    data() {
        return {
            PerPage:'每頁'
            , PageTiems:'筆'
            , Page:'第'
            , Times:'頁'
            , Total:'共'
            , TotalPage:'頁'
        }
    }
    , props: ['pagination']
    , template: `
        <div style="text-align:right">
            <span v-for="pageNo in pagination.pages">
                <a v-if="pagination.pageNo != pageNo" v-on:click="gotoPage(pageNo)" style="cursor:pointer">
                    {{ pageNo }}
                </a>
                <label v-else>
                    {{ "[" + pageNo + "]" }}
                </label>&nbsp;
            </span>
            <span class="pager-nav">
                【{{PerPage}}&nbsp;<input type="text" maxlength="3" style="width:35px;text-align:center;font-size:12px;" name="pageSize" :value="pagination.pageSize" v-on:change="onchange"/>
                &nbsp;{{PageTiems}}，
                {{Total}} {{pagination.totalPage}} {{TotalPage}} {{pagination.totalCount}} {{PageTiems}}】
                <button type="button" class="btn btn-secondary btn-sm pager-btn" style="margin-bottom: 5px;margin-right:5px;" v-on:click="gotoPage()">Q</button>
            </span>
        </div>`
    , methods: {
        gotoPage(pageNo) {
            var self = this;
            console.log(pageNo);
            // 是否有傳入指定頁數
            if (pageNo !== undefined) {
                if (pageNo === '<') {
                    self.pagination.pageNo = parseInt(self.pagination.pageNo) - 1;
                } else if (pageNo === '>') {
                    self.pagination.pageNo = parseInt(self.pagination.pageNo) + 1;
                } else if (pageNo === '<<') {
                    self.pagination.pageNo = (Math.floor((parseInt(self.pagination.pageNo) - 10) / 10) * 10 + 1);
                } else if (pageNo === '>>') {
                    self.pagination.pageNo = (Math.floor((parseInt(self.pagination.pageNo) + 10) / 10) * 10 + 1);
                } else {
                    self.pagination.pageNo = parseInt(pageNo);
                }
            } else {
                self.pagination.pageNo = 1;
            }
            // 指定頁數為0，自動變更為1
            if (parseInt(self.pagination.pageNo) === 0 || self.IsNumeric(self.pagination.pageNo) === false) {
                self.pagination.pageNo = 1;
            }
            // 指定頁數大於總頁數，自動變更為總頁數
            self.pagination.pageNo =
                parseInt(self.pagination.pageNo) > parseInt(self.pagination.totalPage)
                    ? self.pagination.totalPage : self.pagination.pageNo;
            // 指定筆數為0，自動變更為10
            if (parseInt(self.pagination.pageSize) === 0 || self.IsNumeric(self.pagination.pageSize) === false) {
                self.pagination.pageSize = 10;
            }
            // call on even
            this.$emit('requery', { pagination: self.pagination });
        }
        , onchange(e) {
            var self = this;
            var re = /[^0-9]/;
            if (re.test(e.target.value) === false) {
                self.pagination[e.target.name] = parseInt(e.target.value);
            }
        }
        , IsNumeric(n) {
            return (n - 0) === n && n.toString().length > 0;
        }
    }
};